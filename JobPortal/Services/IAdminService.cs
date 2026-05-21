using JobPortal.Models;

namespace JobPortal.Services
{
    public interface IAdminService
    {
        // Returns total count of company accounts
        Task<int> GetTotalCompaniesAsync();

        // Returns total count of applicant accounts
        Task<int> GetTotalApplicantsAsync();

        // Returns total count of active job listings
        Task<int> GetTotalActiveJobsAsync();

        // Returns total count of all submitted applications
        Task<int> GetTotalApplicationsAsync();

        // Returns all users ordered by newest first
        Task<IEnumerable<User>> GetAllUsersAsync();

        // Returns a single user by ID
        Task<User> GetUserByIdAsync(int userId);

        // Deletes a user if no linked data exists
        Task<bool> DeleteUserAsync(int userId);

        // Returns all categories
        Task<IEnumerable<JobCategory>> GetAllCategoriesAsync();

        // Creates a new category
        Task<int> CreateCategoryAsync(string categoryName);

        // Updates a category name
        Task UpdateCategoryAsync(int categoryId, string newName);

        // Deletes a category if unused
        Task<bool> DeleteCategoryAsync(int categoryId);

        // Returns all job postings
        Task<IEnumerable<JobPosting>> GetAllJobPostingsAsync();

        // Permanently deletes a job posting and applications
        Task DeleteJobPostingAsync(int jobId);
    }
}