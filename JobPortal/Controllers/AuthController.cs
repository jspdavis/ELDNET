using Microsoft.AspNetCore.Mvc;
using JobPortal.Models.ViewModels;
using JobPortal.Services;

namespace JobPortal.Controllers
{
    /// <summary>
    /// Handles user registration, login, and logout.
    /// No authentication required — these are public endpoints.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // ── GET /Auth/Login ──────────────────────────────────────────────────
        /// <summary>
        /// Displays the login form.
        /// If already logged in, redirect to the appropriate dashboard.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Redirect already-logged-in users away from the login page
            if (HttpContext.Session.GetString("userId") != null)
            {
                var role = HttpContext.Session.GetString("userRole");
                if (role == "admin") return RedirectToAction("Dashboard", "Admin");
                return role == "company"
                    ? RedirectToAction("Dashboard", "Company")
                    : RedirectToAction("Index", "Applications");
            }

            return View(new LoginViewModel());
        }

        // ── POST /Auth/Login ─────────────────────────────────────────────────
        /// <summary>
        /// Validates credentials and creates a session on success.
        /// Redirects by role: company → Dashboard, applicant → Browse Jobs.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Validate all model annotations before proceeding
            if (!ModelState.IsValid)
                return View(model);

            // Look up the user by email
            var user = await _userService.GetByEmailAsync(model.Email);

            // Verify the BCrypt hash matches the submitted password
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Store user info in session for use across requests
            HttpContext.Session.SetString("userId",   user.UserId.ToString());
            HttpContext.Session.SetString("userRole", user.Role);
            HttpContext.Session.SetString("fullName", user.FullName);

            // Redirect to role-specific page
            if (user.Role == "admin") return RedirectToAction("Dashboard", "Admin");
            return user.Role == "company"
                ? RedirectToAction("Dashboard", "Company")
                : RedirectToAction("Index", "Jobs");
        }

        // ── GET /Auth/Register ───────────────────────────────────────────────
        /// <summary>
        /// Displays the registration form where users choose company or applicant role.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("userId") != null)
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        // ── POST /Auth/Register ──────────────────────────────────────────────
        /// <summary>
        /// Creates a new user account. If role is 'company', also creates a company profile.
        /// Logs the user in immediately after successful registration.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // If registering as a company, company name is required
            if (model.Role == "company" && string.IsNullOrWhiteSpace(model.CompanyName))
                ModelState.AddModelError("CompanyName", "Company name is required for company accounts.");

            // Validate all model annotations before proceeding
            if (!ModelState.IsValid)
                return View(model);

            // Check if email is already taken
            var existing = await _userService.GetByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            // Create the user record (password hashing happens inside the service)
            int newUserId = await _userService.CreateUserAsync(model.Email, model.Password, model.Role, model.FullName);

            // If the new user is a company, create their company profile
            if (model.Role == "company")
            {
                await _userService.CreateCompanyProfileAsync(
                    newUserId,
                    model.CompanyName!,
                    model.CompanyDescription ?? string.Empty,
                    model.CompanyLocation    ?? string.Empty,
                    model.CompanyWebsite     ?? string.Empty
                );
            }

            // Auto-login: set session variables
            HttpContext.Session.SetString("userId",   newUserId.ToString());
            HttpContext.Session.SetString("userRole", model.Role);
            HttpContext.Session.SetString("fullName", model.FullName);

            TempData["Success"] = "Welcome! Your account has been created.";

            return model.Role == "company"
                ? RedirectToAction("Dashboard", "Company")
                : RedirectToAction("Index", "Jobs");
        }

        // ── GET /Auth/Logout ─────────────────────────────────────────────────
        /// <summary>
        /// Clears the session and redirects to the home page.
        /// </summary>
        [HttpGet]
        public IActionResult Logout()
        {
            // Remove all session data
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}
