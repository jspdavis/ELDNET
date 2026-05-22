using Microsoft.AspNetCore.Mvc;
using JobPortal.Services;
using JobPortal.Models.ViewModels;
using System.Threading.Tasks;

namespace JobPortal.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET /Profile — routes to the correct profile view based on role
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("userId");
            if (userIdStr == null) return RedirectToAction("Login", "Auth");
            int userId = int.Parse(userIdStr);
            var role = HttpContext.Session.GetString("userRole");

            if (role == "applicant")
            {
                var profile = await _profileService.GetApplicantProfileAsync(userId);
                return View("~/Views/Profile/ApplicantProfile.cshtml", profile);
            }
            else if (role == "company")
            {
                var profile = await _profileService.GetCompanyProfileAsync(userId);
                return View("~/Views/Profile/CompanyProfile.cshtml", profile);
            }
            else if (role == "admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            
            return RedirectToAction("Login", "Auth");
        }

        // GET /Profile/Edit — loads the edit form pre-filled with current data
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userIdStr = HttpContext.Session.GetString("userId");
            if (userIdStr == null) return RedirectToAction("Login", "Auth");
            int userId = int.Parse(userIdStr);
            var role = HttpContext.Session.GetString("userRole");

            if (role == "applicant")
            {
                var profile = await _profileService.GetApplicantProfileAsync(userId);
                var model = new EditApplicantProfileViewModel
                {
                    FullName = profile?.FullName,
                    Phone = profile?.Phone,
                    Location = profile?.Location,
                    Bio = profile?.Bio
                };
                return View("~/Views/Profile/EditApplicant.cshtml", model);
            }
            else if (role == "company")
            {
                var profile = await _profileService.GetCompanyProfileAsync(userId);
                var model = new EditCompanyProfileViewModel
                {
                    FullName = profile?.FullName,
                    CompanyName = profile?.CompanyName,
                    Description = profile?.Description,
                    Location = profile?.Location,
                    Website = profile?.Website
                };
                return View("~/Views/Profile/EditCompany.cshtml", model);
            }

            return RedirectToAction("Login", "Auth");
        }

        // POST /Profile/Edit — saves changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost()
        {
            var userIdStr = HttpContext.Session.GetString("userId");
            if (userIdStr == null) return RedirectToAction("Login", "Auth");
            int userId = int.Parse(userIdStr);
            var role = HttpContext.Session.GetString("userRole");

            if (role == "applicant")
            {
                var model = new EditApplicantProfileViewModel();
                if (await TryUpdateModelAsync(model))
                {
                    await _profileService.UpdateApplicantProfileAsync(userId, model);
                    HttpContext.Session.SetString("fullName", model.FullName);
                    TempData["Success"] = "Profile updated successfully.";
                    return RedirectToAction("Index");
                }
                return View("~/Views/Profile/EditApplicant.cshtml", model);
            }
            else if (role == "company")
            {
                var model = new EditCompanyProfileViewModel();
                if (await TryUpdateModelAsync(model))
                {
                    await _profileService.UpdateCompanyProfileAsync(userId, model);
                    HttpContext.Session.SetString("fullName", model.FullName);
                    TempData["Success"] = "Profile updated successfully.";
                    return RedirectToAction("Index");
                }
                return View("~/Views/Profile/EditCompany.cshtml", model);
            }

            return RedirectToAction("Login", "Auth");
        }
    }
}
