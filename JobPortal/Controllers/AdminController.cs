using JobPortal.Models;
using JobPortal.Models.ViewModels;
using JobPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Checks whether the current session user is an admin
        private bool IsAdmin() =>
            HttpContext.Session.GetString("userRole") == "admin";

        // Displays the admin dashboard with system statistics
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var vm = new AdminDashboardViewModel
            {
                TotalCompanies = await _adminService.GetTotalCompaniesAsync(),
                TotalApplicants = await _adminService.GetTotalApplicantsAsync(),
                TotalActiveJobs = await _adminService.GetTotalActiveJobsAsync(),
                TotalApplications = await _adminService.GetTotalApplicationsAsync()
            };

            return View(vm);
        }

        // Displays all registered users except admins
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        // Deletes a user if no linked jobs or applications exist
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var deleted = await _adminService.DeleteUserAsync(id);

            if (!deleted)
            {
                TempData["Error"] =
                    "Cannot delete user with existing jobs or applications.";
            }
            else
            {
                TempData["Success"] = "User deleted successfully.";
            }

            return RedirectToAction(nameof(Users));
        }

        // Displays all job categories
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var categories = await _adminService.GetAllCategoriesAsync();
            return View(categories);
        }

        // Creates a new job category
        [HttpPost]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToAction(nameof(Categories));
            }

            await _adminService.CreateCategoryAsync(categoryName);

            TempData["Success"] = "Category created.";

            return RedirectToAction(nameof(Categories));
        }

        // Updates an existing category
        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string newName)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            await _adminService.UpdateCategoryAsync(id, newName);

            TempData["Success"] = "Category updated.";

            return RedirectToAction(nameof(Categories));
        }

        // Deletes a category if no jobs reference it
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var deleted = await _adminService.DeleteCategoryAsync(id);

            if (!deleted)
            {
                TempData["Error"] =
                    "Cannot delete a category with active job listings.";
            }
            else
            {
                TempData["Success"] = "Category deleted.";
            }

            return RedirectToAction(nameof(Categories));
        }

        // Displays all job postings
        public async Task<IActionResult> Jobs()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var jobs = await _adminService.GetAllJobPostingsAsync();

            return View(jobs);
        }

        // Permanently deletes a job posting and its applications
        [HttpPost]
        public async Task<IActionResult> DeleteJob(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            await _adminService.DeleteJobPostingAsync(id);

            TempData["Success"] =
                "Job listing permanently deleted.";

            return RedirectToAction(nameof(Jobs));
        }
    }
}