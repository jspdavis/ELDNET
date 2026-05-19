using System.Collections.Generic;
using JobPortal.Models;

namespace JobPortal.Services
{
    /// <summary>
    /// Contract for application submission and tracking operations.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>Submits a new application for a job.</summary>
        Task ApplyAsync(int jobId, int userId, string coverLetter);

        /// <summary>Returns all applications submitted by a specific applicant.</summary>
        Task<IEnumerable<Application>> GetByApplicantAsync(int userId);

        /// <summary>Returns all applications received for a specific job (company view).</summary>
        Task<IEnumerable<Application>> GetByJobAsync(int jobId);

        /// <summary>Updates the status of an application (reviewed, accepted, rejected).</summary>
        Task UpdateStatusAsync(int applicationId, string status);

        /// <summary>
        /// Checks whether a specific user has already applied to a specific job.
        /// Used to prevent duplicate applications.
        /// </summary>
        Task<bool> HasAppliedAsync(int jobId, int userId);
    }
}
