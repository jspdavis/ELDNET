using Microsoft.AspNetCore.Mvc;
using JobPortal.Models.ViewModels;
using JobPortal.Services;
using JobPortal.Helpers;
using JobPortal.Settings;

namespace JobPortal.Controllers
{
    /// <summary>
    /// Handles application submission and tracking for applicants,
    /// and cover letter file downloads for companies.
    /// All actions require the user to be logged in.
    /// </summary>
    public class ApplicationsController : Controller
    {
        private readonly IApplicationService _appService;
        private readonly IJobService         _jobService;
        private readonly FileStorageSettings _storage;

        public ApplicationsController(
            IApplicationService appService,
            IJobService         jobService,
            FileStorageSettings storage)
        {
            _appService = appService;
            _jobService = jobService;
            _storage    = storage;
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
        /// Submits the application. Accepts either a typed cover letter OR an uploaded
        /// cover letter PDF — not both required. Both empty is also valid (FR-20).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Applications/Apply/{jobId}")]
        public async Task<IActionResult> Apply(int jobId, ApplicationViewModel model,
            IFormFile? coverLetterFile)
        {
            if (!IsApplicant()) return RedirectToAction("Login", "Auth");

            // Validate only non-file model fields (CoverLetter is optional)
            ModelState.Remove("CoverLetter");
            if (!ModelState.IsValid)
                return View(model);

            int userId = int.Parse(HttpContext.Session.GetString("userId")!);

            // Check again to prevent race conditions
            if (await _appService.HasAppliedAsync(jobId, userId))
            {
                TempData["Error"] = "You have already applied to this job.";
                return RedirectToAction("Index");
            }

            string? coverLetterFilename     = null;
            string? coverLetterOriginalName = null;

            // If a file was provided and passes validation, save it and use it as the cover letter
            if (coverLetterFile != null && coverLetterFile.Length > 0)
            {
                if (FileUploadHelper.IsValidDocument(coverLetterFile, out var fileError))
                {
                    coverLetterFilename     = FileUploadHelper.GenerateUniqueFilename(coverLetterFile.FileName);
                    coverLetterOriginalName = coverLetterFile.FileName;

                    var savePath = Path.Combine(_storage.AppDataPath, "cover_letters", coverLetterFilename);
                    using var stream = new FileStream(savePath, FileMode.Create);
                    await coverLetterFile.CopyToAsync(stream);
                }
                else
                {
                    // File failed validation — fall back to typed text (no hard error)
                    TempData["Warning"] = $"Cover letter file rejected: {fileError} Your typed cover letter was used instead.";
                }
            }

            // Submit application with whichever cover letter option was provided
            await _appService.ApplyWithFileAsync(
                jobId, userId,
                model.CoverLetter,
                coverLetterFilename,
                coverLetterOriginalName);

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

        // ── GET /Applications/DownloadCoverLetter/{applicationId} ────────────
        /// <summary>
        /// Company-side: serves the uploaded PDF cover letter for a specific application.
        /// Verifies the job belongs to this company before serving the file.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadCoverLetter(int applicationId)
        {
            // Only company users may download cover letters
            var userIdStr = HttpContext.Session.GetString("userId");
            if (userIdStr == null || HttpContext.Session.GetString("userRole") != "company")
                return RedirectToAction("Login", "Auth");

            int companyUserId = int.Parse(userIdStr);

            // Retrieve application and verify it belongs to this company (security check)
            var application = await _appService.GetApplicationForCompanyAsync(applicationId, companyUserId);
            if (application == null || string.IsNullOrEmpty(application.CoverLetterFilename))
                return NotFound();

            var fullPath = Path.Combine(_storage.AppDataPath, "cover_letters", application.CoverLetterFilename);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            // Cover letters are always PDF (only PDF accepted on the upload form)
            const string contentType = "application/pdf";
            var downloadName = application.CoverLetterOriginalName ?? application.CoverLetterFilename;

            return File(System.IO.File.ReadAllBytes(fullPath), contentType, downloadName);
        }

        // ── Helper: Check if current session user is an applicant ─────────────
        private bool IsApplicant() =>
            HttpContext.Session.GetString("userId") != null &&
            HttpContext.Session.GetString("userRole") == "applicant";
    }
}
