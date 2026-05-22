using Microsoft.AspNetCore.Mvc;
using JobPortal.Services;
using JobPortal.Models.ViewModels;
using JobPortal.Helpers;
using JobPortal.Settings;

namespace JobPortal.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileService      _profileService;
        private readonly FileStorageSettings  _storage;

        public ProfileController(IProfileService profileService, FileStorageSettings storage)
        {
            _profileService = profileService;
            _storage        = storage;
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
                    Phone    = profile?.Phone,
                    Location = profile?.Location,
                    Bio      = profile?.Bio
                };
                return View("~/Views/Profile/EditApplicant.cshtml", model);
            }
            else if (role == "company")
            {
                var profile = await _profileService.GetCompanyProfileAsync(userId);
                var model = new EditCompanyProfileViewModel
                {
                    FullName    = profile?.FullName,
                    CompanyName = profile?.CompanyName,
                    Description = profile?.Description,
                    Location    = profile?.Location,
                    Website     = profile?.Website
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

        // GET /Profile/DownloadResume
        // Serves the applicant's resume file through the controller (never exposes the file path directly)
        [HttpGet]
        public async Task<IActionResult> DownloadResume()
        {
            var userIdStr = HttpContext.Session.GetString("userId");
            if (userIdStr == null || HttpContext.Session.GetString("userRole") != "applicant")
                return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdStr);

            // Retrieve the stored server-side filename from the database
            var filename = await _profileService.GetResumeFilenameAsync(userId);
            if (string.IsNullOrEmpty(filename))
            {
                TempData["Error"] = "No resume uploaded.";
                return RedirectToAction("Index");
            }

            var fullPath = Path.Combine(_storage.AppDataPath, "resumes", filename);
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = "Resume file not found on server.";
                return RedirectToAction("Index");
            }

            // Determine content type from extension
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            var contentType = ext == ".pdf"
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            // Derive the original name from the stored filename (strip the GUID prefix)
            var profile = await _profileService.GetApplicantProfileAsync(userId);
            var downloadName = profile?.ResumeOriginalName ?? filename;

            return File(System.IO.File.ReadAllBytes(fullPath), contentType, downloadName);
        }

        // POST /Profile/UploadResume
        // Validates, saves, and records the uploaded resume file for the applicant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadResume(IFormFile resumeFile)
        {
            var userIdStr = HttpContext.Session.GetString("userId");
            if (userIdStr == null || HttpContext.Session.GetString("userRole") != "applicant")
                return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdStr);

            if (resumeFile == null || resumeFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }

            // Validate MIME type and size
            if (!FileUploadHelper.IsValidDocument(resumeFile, out var errorMessage))
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("Index");
            }

            // If the applicant already has a resume, delete the old file and clear DB record
            var existingFilename = await _profileService.GetResumeFilenameAsync(userId);
            if (!string.IsNullOrEmpty(existingFilename))
            {
                var oldPath = Path.Combine(_storage.AppDataPath, "resumes", existingFilename);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                await _profileService.ClearResumeAsync(userId);
            }

            // Generate a unique filename and save to disk
            var uniqueFilename = FileUploadHelper.GenerateUniqueFilename(resumeFile.FileName);
            var savePath = Path.Combine(_storage.AppDataPath, "resumes", uniqueFilename);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await resumeFile.CopyToAsync(stream);
            }

            // Persist filename and original name to the database
            await _profileService.SaveResumeAsync(userId, uniqueFilename, resumeFile.FileName);

            TempData["Success"] = "Resume uploaded successfully.";
            return RedirectToAction("Index");
        }
    }
}
