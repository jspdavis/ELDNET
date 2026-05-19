using Microsoft.AspNetCore.Mvc;
using JobPortal.Models.ViewModels;
using JobPortal.Services;

namespace JobPortal.Controllers
{
    /// <summary>
    /// Handles job listing browsing (public), job creation and management (company only).
    /// </summary>
    public class JobsController : Controller
    {
        private readonly IJobService      _jobService;
        private readonly IUserService     _userService;
        private readonly IApplicationService _appService;

        public JobsController(IJobService jobService, IUserService userService, IApplicationService appService)
        {
            _jobService  = jobService;
            _userService = userService;
            _appService  = appService;
        }

        // ── GET /Jobs ────────────────────────────────────────────────────────
        /// <summary>
        /// Public job listings page. Supports keyword search via query string.
        /// Category filtering is done via AJAX (see Filter action below).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword)
        {
            var jobs       = await _jobService.SearchJobsAsync(keyword, null);
            var categories = await _jobService.GetAllCategoriesAsync();

            ViewBag.Keyword    = keyword;
            ViewBag.Categories = categories;

            return View(jobs);
        }

        // ── GET /Jobs/Filter ─────────────────────────────────────────────────
        /// <summary>
        /// AJAX endpoint: returns a partial view of job cards filtered by category/keyword.
        /// Called from fetch() in site.js when a category button is clicked.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Filter(int? categoryId, string? keyword)
        {
            // Search with the given filters
            var jobs = await _jobService.SearchJobsAsync(keyword, categoryId);

            // Return just the partial HTML for the job cards container
            return PartialView("_JobCard", jobs);
        }

        // ── GET /Jobs/Details/{id} ───────────────────────────────────────────
        /// <summary>
        /// Full detail view for a single job posting.
        /// Shows the Apply button to logged-in applicants who haven't applied yet.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null) return NotFound();

            // Check if the current applicant has already applied
            var userIdStr = HttpContext.Session.GetString("userId");
            bool hasApplied = false;

            if (userIdStr != null && HttpContext.Session.GetString("userRole") == "applicant")
            {
                hasApplied = await _appService.HasAppliedAsync(id, int.Parse(userIdStr));
            }

            ViewBag.HasApplied = hasApplied;
            return View(job);
        }

        // ── GET /Jobs/Create ─────────────────────────────────────────────────
        /// <summary>
        /// Shows the job creation form. Only accessible to company users.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Protect: only companies can post jobs
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            var categories = await _jobService.GetAllCategoriesAsync();
            return View(new JobFormViewModel { Categories = categories.ToList() });
        }

        // ── POST /Jobs/Create ────────────────────────────────────────────────
        /// <summary>
        /// Processes the job creation form.
        /// Looks up the company_id from the logged-in user before inserting.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobFormViewModel model)
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                // Re-populate category dropdown if form fails validation
                model.Categories = (await _jobService.GetAllCategoriesAsync()).ToList();
                return View(model);
            }

            int userId  = int.Parse(HttpContext.Session.GetString("userId")!);
            var company = await _userService.GetCompanyByUserIdAsync(userId);
            if (company == null)
            {
                TempData["Error"] = "Company profile not found. Please complete your profile first.";
                return RedirectToAction("Profile", "Company");
            }

            // Create the job posting
            await _jobService.CreateJobAsync(
                company.CompanyId,
                model.CategoryId,
                model.Title,
                model.Description,
                model.Requirements,
                model.Location,
                model.SalaryRange
            );

            TempData["Success"] = "Job posted successfully!";
            return RedirectToAction("Dashboard", "Company");
        }

        // ── GET /Jobs/Edit/{id} ──────────────────────────────────────────────
        /// <summary>
        /// Shows the job edit form pre-filled with existing data.
        /// Only the company that owns the job can edit it.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null) return NotFound();

            // Verify ownership: the logged-in company must own this job
            int userId  = int.Parse(HttpContext.Session.GetString("userId")!);
            var company = await _userService.GetCompanyByUserIdAsync(userId);
            if (company == null || company.CompanyId != job.CompanyId)
                return Forbid();

            var categories = await _jobService.GetAllCategoriesAsync();

            var model = new JobFormViewModel
            {
                JobId        = job.JobId,
                Title        = job.Title,
                Description  = job.Description,
                Requirements = job.Requirements,
                Location     = job.Location,
                SalaryRange  = job.SalaryRange,
                CategoryId   = job.CategoryId,
                Categories   = categories.ToList()
            };

            return View(model);
        }

        // ── POST /Jobs/Edit/{id} ─────────────────────────────────────────────
        /// <summary>
        /// Processes the job edit form and updates the record.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobFormViewModel model)
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                model.Categories = (await _jobService.GetAllCategoriesAsync()).ToList();
                return View(model);
            }

            // Double-check ownership before updating
            var job     = await _jobService.GetJobByIdAsync(id);
            int userId  = int.Parse(HttpContext.Session.GetString("userId")!);
            var company = await _userService.GetCompanyByUserIdAsync(userId);

            if (job == null || company == null || company.CompanyId != job.CompanyId)
                return Forbid();

            await _jobService.UpdateJobAsync(
                id,
                model.CategoryId,
                model.Title,
                model.Description,
                model.Requirements,
                model.Location,
                model.SalaryRange
            );

            TempData["Success"] = "Job updated successfully!";
            return RedirectToAction("Dashboard", "Company");
        }

        // ── POST /Jobs/Delete/{id} ───────────────────────────────────────────
        /// <summary>
        /// Soft-deletes a job by setting is_active = false.
        /// Only the owning company can delete their own jobs.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsCompany()) return RedirectToAction("Login", "Auth");

            // Verify ownership before deleting
            var job     = await _jobService.GetJobByIdAsync(id);
            int userId  = int.Parse(HttpContext.Session.GetString("userId")!);
            var company = await _userService.GetCompanyByUserIdAsync(userId);

            if (job == null || company == null || company.CompanyId != job.CompanyId)
                return Forbid();

            await _jobService.DeleteJobAsync(id);

            TempData["Success"] = "Job removed from listings.";
            return RedirectToAction("Dashboard", "Company");
        }

        // ── Helper: Check if current session user is a company ────────────────
        private bool IsCompany() =>
            HttpContext.Session.GetString("userId") != null &&
            HttpContext.Session.GetString("userRole") == "company";
    }
}
