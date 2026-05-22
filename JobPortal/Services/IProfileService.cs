using JobPortal.Models;
using JobPortal.Models.ViewModels;

namespace JobPortal.Services
{
    public interface IProfileService
    {
        // Returns the full applicant profile joined with users table
        Task<ApplicantProfile> GetApplicantProfileAsync(int userId);

        // Updates full_name in users table AND phone, location, bio in applicant_profiles. Wrap both updates in a transaction.
        Task UpdateApplicantProfileAsync(int userId, EditApplicantProfileViewModel model);

        // Returns the full company profile joined with users table
        Task<CompanyProfile> GetCompanyProfileAsync(int userId);

        // Updates full_name in users table AND company_name, description, location, website in companies table. Wrap in a transaction.
        Task UpdateCompanyProfileAsync(int userId, EditCompanyProfileViewModel model);

        // Saves resume filename and metadata to applicant_profiles
        Task SaveResumeAsync(int userId, string filename, string originalName);

        // Returns the stored resume filename for a user (or null if none uploaded)
        Task<string?> GetResumeFilenameAsync(int userId);

        // Clears resume data when a user replaces their resume
        Task ClearResumeAsync(int userId);
    }
}
