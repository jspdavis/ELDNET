using Microsoft.AspNetCore.Mvc;
using JobPortal.Services;
using JobPortal.Models;

namespace JobPortal.Controllers
{
    /// <summary>
    /// Handles the company dashboard, profile editing, and applicant management.
    /// All actions require the user to be logged in as a company.
    /// </summary>
    public class CompanyController : Controller
    {
        private readonly IUserService        _userService;
        private readonly IJobService         _jobService;
        private readonly IApplicationService _appService;

        public CompanyController(IUserService userService, IJobService jobService, IApplicationService appService)
        {
            _userService = userService;
            _jobService  = jobService;
            _appService  = appService;
        }

        // ── GET /Company/Dashboard ───────────────────────────────────────────
        /// <summary>
        /// Company dashboard: shows all jobs posted by this company
        /// along with how many applicants each job has received.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            int userId  = int.Parse(HttpContext.Session.GetString("userId")!);
            var company = await _userService.GetCompanyByUserIdAsync(userId);

            if (company == null)
            {
                TempData["Error"] = "Please complete your company profile first.";
                return RedirectToAction("Profile");
            }

            // Fetch all jobs for this company (active and inactive)
            var jobs = await _jobService.GetJobsByCompanyAsync(company.CompanyId);

            ViewBag.Company = company;
            return View(jobs);
        }

        // ── GET /Company/Applicants/{jobId} ──────────────────────────────────
        /// <summary>
        /// Shows a list of applicants for a specific job posting.
        /// Only the owning company can view applicants for their jobs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Applicants(int jobId)
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            // Verify this job belongs to the logged-in company
            int userId  = int.Parse(HttpContext.Session.GetString("userId")!);
            var company = await _userService.GetCompanyByUserIdAsync(userId);
            var job     = await _jobService.GetJobByIdAsync(jobId);

            if (company == null || job == null || job.CompanyId != company.CompanyId)
                return Forbid();

            // Get all applications for this job
            var applications = await _appService.GetByJobAsync(jobId);

            ViewBag.Job = job;
            return View(applications);
        }

        // ── POST /Company/UpdateStatus/{applicationId} ────────────────────────
        /// <summary>
        /// Updates the status of an application (reviewed, accepted, rejected).
        /// Redirects back to the applicants page for that job.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int applicationId, string status, int jobId)
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            // Only allow valid status values
            var validStatuses = new[] { "pending", "reviewed", "accepted", "rejected" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction("Applicants", new { jobId });
            }

            await _appService.UpdateStatusAsync(applicationId, status);

            TempData["Success"] = $"Application status updated to '{status}'.";
            return RedirectToAction("Applicants", new { jobId });
        }

        // ── GET /Company/Profile ─────────────────────────────────────────────
        /// <summary>
        /// Shows the company's profile edit form.
        /// Pre-populates with existing data if the profile exists.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            int userId  = int.Parse(HttpContext.Session.GetString("userId")!);
            var company = await _userService.GetCompanyByUserIdAsync(userId);

            // Pass an empty Company model if no profile exists yet
            return View(company ?? new Company());
        }

        // ── POST /Company/Profile ────────────────────────────────────────────
        /// <summary>
        /// Saves changes to the company profile.
        /// Creates the profile if it doesn't exist yet; updates it otherwise.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Company model)
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(model.CompanyName))
            {
                ModelState.AddModelError("CompanyName", "Company name is required.");
                return View(model);
            }

            int userId      = int.Parse(HttpContext.Session.GetString("userId")!);
            var existingCo  = await _userService.GetCompanyByUserIdAsync(userId);

            if (existingCo == null)
            {
                // First time creating the profile
                await _userService.CreateCompanyProfileAsync(
                    userId,
                    model.CompanyName,
                    model.Description,
                    model.Location,
                    model.Website
                );
            }
            else
            {
                // Update the existing profile
                await _userService.UpdateCompanyProfileAsync(
                    existingCo.CompanyId,
                    model.CompanyName,
                    model.Description,
                    model.Location,
                    model.Website
                );
            }

            TempData["Success"] = "Company profile saved successfully!";
            return RedirectToAction("Dashboard");
        }

        // ── Helper: Check if current session user is a company ────────────────
        private bool IsCompany() =>
            HttpContext.Session.GetString("userId") != null &&
            HttpContext.Session.GetString("userRole") == "company";
    }
}
