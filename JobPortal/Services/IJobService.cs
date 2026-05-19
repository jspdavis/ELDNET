using System.Collections.Generic;
using JobPortal.Models;

namespace JobPortal.Services
{
    /// <summary>
    /// Contract for all job posting operations.
    /// </summary>
    public interface IJobService
    {
        /// <summary>Returns all active job postings (for the public listings page).</summary>
        Task<IEnumerable<JobPosting>> GetAllActiveJobsAsync();

        /// <summary>Returns active jobs filtered by keyword and/or category.</summary>
        Task<IEnumerable<JobPosting>> SearchJobsAsync(string? keyword, int? categoryId);

        /// <summary>Returns full details for a single job posting.</summary>
        Task<JobPosting?> GetJobByIdAsync(int jobId);

        /// <summary>Returns all jobs posted by a specific company.</summary>
        Task<IEnumerable<JobPosting>> GetJobsByCompanyAsync(int companyId);

        /// <summary>Creates a new job posting and returns the new job_id.</summary>
        Task<int> CreateJobAsync(int companyId, int categoryId, string title, string description,
                                  string requirements, string location, string salaryRange);

        /// <summary>Updates an existing job posting.</summary>
        Task UpdateJobAsync(int jobId, int categoryId, string title, string description,
                             string requirements, string location, string salaryRange);

        /// <summary>Soft-deletes a job by setting is_active = false.</summary>
        Task DeleteJobAsync(int jobId);

        /// <summary>Returns all job categories.</summary>
        Task<IEnumerable<JobCategory>> GetAllCategoriesAsync();
    }
}
