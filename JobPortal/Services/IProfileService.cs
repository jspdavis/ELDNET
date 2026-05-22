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
    }
}
