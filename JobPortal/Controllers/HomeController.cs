using Microsoft.AspNetCore.Mvc;
using JobPortal.Services;

namespace JobPortal.Controllers
{
    /// <summary>
    /// Public-facing home page controller.
    /// No authentication required.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IJobService _jobService;

        public HomeController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // ── GET / ────────────────────────────────────────────────────────────
        /// <summary>
        /// Landing page. Loads the 6 most recently posted active jobs
        /// to give visitors a preview of available positions.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Fetch a small preview of active jobs for the hero section
            var jobs = (await _jobService.GetAllActiveJobsAsync()).Take(6);
            return View(jobs);
        }

        // ── GET /Home/About ──────────────────────────────────────────────────
        /// <summary>
        /// Static about/contact page with information about the portal.
        /// </summary>
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        // ── GET /Home/Error ──────────────────────────────────────────────────
        /// <summary>
        /// Generic error page shown when an unhandled exception occurs.
        /// </summary>
        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
    }
}
