using Dapper;
using JobPortal.Data;
using JobPortal.Models;

namespace JobPortal.Services
{
    /// <summary>
    /// Handles all application-related database operations using Dapper.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly DatabaseContext _context;

        public ApplicationService(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Submits a new application for a job posting.
        /// If the user has already applied, the DB unique constraint will throw.
        /// </summary>
        public async Task ApplyAsync(int jobId, int userId, string coverLetter)
        {
            using var db = _context.CreateConnection();

            // Insert a new application with default status 'pending'
            const string sql = @"
                INSERT INTO applications (job_id, user_id, cover_letter, status, applied_at)
                VALUES (@JobId, @UserId, @CoverLetter, 'pending', NOW())";

            await db.ExecuteAsync(sql, new
            {
                JobId       = jobId,
                UserId      = userId,
                CoverLetter = coverLetter
            });
        }

        /// <summary>
        /// Returns all applications submitted by a specific applicant.
        /// Joins job and company data for display.
        /// </summary>
        public async Task<IEnumerable<Application>> GetByApplicantAsync(int userId)
        {
            using var db = _context.CreateConnection();

            // Fetch all applications for this user with job title and company name
            const string sql = @"
                SELECT a.application_id AS ApplicationId,
                       a.job_id         AS JobId,
                       a.user_id        AS UserId,
                       a.cover_letter   AS CoverLetter,
                       a.status         AS Status,
                       a.applied_at     AS AppliedAt,
                       jp.title         AS JobTitle,
                       c.company_name   AS CompanyName
                FROM applications a
                INNER JOIN job_postings jp ON jp.job_id    = a.job_id
                INNER JOIN companies   c  ON c.company_id = jp.company_id
                WHERE a.user_id = @UserId
                ORDER BY a.applied_at DESC";

            return await db.QueryAsync<Application>(sql, new { UserId = userId });
        }

        /// <summary>
        /// Returns all applications received for a specific job posting.
        /// Used on the Company's Applicants page.
        /// </summary>
        public async Task<IEnumerable<Application>> GetByJobAsync(int jobId)
        {
            using var db = _context.CreateConnection();

            // Fetch all applicants for this job with applicant name and email
            const string sql = @"
                SELECT a.application_id AS ApplicationId,
                       a.job_id         AS JobId,
                       a.user_id        AS UserId,
                       a.cover_letter   AS CoverLetter,
                       a.status         AS Status,
                       a.applied_at     AS AppliedAt,
                       u.full_name      AS ApplicantName,
                       u.email          AS ApplicantEmail
                FROM applications a
                INNER JOIN users u ON u.user_id = a.user_id
                WHERE a.job_id = @JobId
                ORDER BY a.applied_at ASC";

            return await db.QueryAsync<Application>(sql, new { JobId = jobId });
        }

        /// <summary>
        /// Updates the status of an application.
        /// Valid statuses: 'pending', 'reviewed', 'accepted', 'rejected'.
        /// </summary>
        public async Task UpdateStatusAsync(int applicationId, string status)
        {
            using var db = _context.CreateConnection();

            // Update just the status column for the given application
            const string sql = @"
                UPDATE applications
                SET status = @Status
                WHERE application_id = @ApplicationId";

            await db.ExecuteAsync(sql, new
            {
                ApplicationId = applicationId,
                Status        = status
            });
        }

        /// <summary>
        /// Checks whether a user has already applied to a job.
        /// Returns true if a record already exists.
        /// </summary>
        public async Task<bool> HasAppliedAsync(int jobId, int userId)
        {
            using var db = _context.CreateConnection();

            // Count applications for this job/user pair; returns 1 if applied, 0 if not
            const string sql = @"
                SELECT COUNT(*) FROM applications
                WHERE job_id = @JobId AND user_id = @UserId";

            int count = await db.ExecuteScalarAsync<int>(sql, new { JobId = jobId, UserId = userId });
            return count > 0;
        }
    }
}
