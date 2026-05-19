using Microsoft.AspNetCore.Mvc;
using JobPortal.Models.ViewModels;
using JobPortal.Services;

namespace JobPortal.Controllers
{
    /// <summary>
    /// Handles application submission and tracking for applicants.
    /// All actions require the user to be logged in as an applicant.
    /// </summary>
    public class ApplicationsController : Controller
    {
        private readonly IApplicationService _appService;
        private readonly IJobService         _jobService;

        public ApplicationsController(IApplicationService appService, IJobService jobService)
        {
            _appService = appService;
            _jobService = jobService;
        }

        // ── GET /Applications ────────────────────────────────────────────────
        /// <summary>
        /// Shows the logged-in applicant's submitted applications and their statuses.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Protect: only applicants can view their own applications
            if (!IsApplicant()) return RedirectToAction("Login", "Auth");

            int userId = int.Parse(HttpContext.Session.GetString("userId")!);

            // Get all applications submitted by this user
            var applications = await _appService.GetByApplicantAsync(userId);

            return View(applications);
        }

        // ── GET /Applications/Apply/{jobId} ──────────────────────────────────
        /// <summary>
        /// Shows the cover letter form for applying to a specific job.
        /// </summary>
        [HttpGet]
        [Route("Applications/Apply/{jobId}")]
        public async Task<IActionResult> Apply(int jobId)
        {
            if (!IsApplicant()) return RedirectToAction("Login", "Auth");

            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null) return NotFound();

            int userId = int.Parse(HttpContext.Session.GetString("userId")!);

            // Prevent double-applications
            if (await _appService.HasAppliedAsync(jobId, userId))
            {
                TempData["Error"] = "You have already applied to this job.";
                return RedirectToAction("Details", "Jobs", new { id = jobId });
            }

            var model = new ApplicationViewModel
            {
                JobId       = job.JobId,
                JobTitle    = job.Title,
                CompanyName = job.CompanyName
            };

            return View(model);
        }

        // ── POST /Applications/Apply/{jobId} ─────────────────────────────────
        /// <summary>
        /// Submits the application (cover letter) to the database.
        /// Redirects to the applicant's applications list on success.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Applications/Apply/{jobId}")]
        public async Task<IActionResult> Apply(int jobId, ApplicationViewModel model)
        {
            if (!IsApplicant()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            int userId = int.Parse(HttpContext.Session.GetString("userId")!);

            // Check again to prevent race conditions
            if (await _appService.HasAppliedAsync(jobId, userId))
            {
                TempData["Error"] = "You have already applied to this job.";
                return RedirectToAction("Index");
            }

            await _appService.ApplyAsync(jobId, userId, model.CoverLetter);

            TempData["Success"] = "Your application has been submitted!";
            return RedirectToAction("Index");
        }

        // ── GET /Applications/Track ──────────────────────────────────────────
        /// <summary>
        /// Alternative status tracking view showing a visual pipeline per application.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Track()
        {
            if (!IsApplicant()) return RedirectToAction("Login", "Auth");

            int userId = int.Parse(HttpContext.Session.GetString("userId")!);
            var applications = await _appService.GetByApplicantAsync(userId);

            return View(applications);
        }

        // ── Helper: Check if current session user is an applicant ─────────────
        private bool IsApplicant() =>
            HttpContext.Session.GetString("userId") != null &&
            HttpContext.Session.GetString("userRole") == "applicant";
    }
}
