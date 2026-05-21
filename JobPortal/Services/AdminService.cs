using Dapper;
using JobPortal.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace JobPortal.Services
{
    public class AdminService : IAdminService
    {
        private readonly IConfiguration _configuration;

        public AdminService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection =>
            new MySqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));

        // Returns total count of company accounts
        public async Task<int> GetTotalCompaniesAsync()
        {
            using var db = Connection;

            return await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM users WHERE role = 'company'");
        }

        // Returns total count of applicant accounts
        public async Task<int> GetTotalApplicantsAsync()
        {
            using var db = Connection;

            return await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM users WHERE role = 'applicant'");
        }

        // Returns total count of active job listings
        public async Task<int> GetTotalActiveJobsAsync()
        {
            using var db = Connection;

            return await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM job_postings WHERE is_active = 1");
        }

        // Returns total count of all submitted applications
        public async Task<int> GetTotalApplicationsAsync()
        {
            using var db = Connection;

            return await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM applications");
        }

        // Returns all users ordered by newest first
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var db = Connection;

            return await db.QueryAsync<User>(@"
                SELECT user_id        AS UserId,
                       email          AS Email,
                       password_hash  AS PasswordHash,
                       role           AS Role,
                       full_name      AS FullName,
                       created_at     AS CreatedAt
                FROM users
                WHERE role IN ('company', 'applicant')
                ORDER BY created_at DESC");
        }

        // Returns a single user by ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var db = Connection;

            return await db.QueryFirstOrDefaultAsync<User>(@"
                SELECT user_id        AS UserId,
                       email          AS Email,
                       password_hash  AS PasswordHash,
                       role           AS Role,
                       full_name      AS FullName,
                       created_at     AS CreatedAt
                FROM users
                WHERE user_id = @UserId",
                new { UserId = userId });
        }

        // Deletes a user if no linked data exists
        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var db = Connection;

            // Check if this user has submitted any applications
            var applications = await db.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM applications
                WHERE user_id = @UserId",
                new { UserId = userId });

            // Check if this user owns a company that has job postings
            var jobs = await db.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM job_postings jp
                INNER JOIN companies c ON c.company_id = jp.company_id
                WHERE c.user_id = @UserId",
                new { UserId = userId });

            if (applications > 0 || jobs > 0)
                return false;

            await db.ExecuteAsync(@"
                DELETE FROM users
                WHERE user_id = @UserId",
                new { UserId = userId });

            return true;
        }

        // Returns all job postings with company and category names
        public async Task<IEnumerable<JobPosting>> GetAllJobPostingsAsync()
        {
            using var db = Connection;

            return await db.QueryAsync<JobPosting>(@"
                SELECT jp.job_id        AS JobId,
                       jp.company_id    AS CompanyId,
                       jp.category_id   AS CategoryId,
                       jp.title         AS Title,
                       jp.description   AS Description,
                       jp.requirements  AS Requirements,
                       jp.location      AS Location,
                       jp.salary_range  AS SalaryRange,
                       jp.is_active     AS IsActive,
                       jp.posted_at     AS PostedAt,
                       c.company_name   AS CompanyName,
                       jc.category_name AS CategoryName
                FROM job_postings jp
                INNER JOIN companies      c  ON c.company_id  = jp.company_id
                INNER JOIN job_categories jc ON jc.category_id = jp.category_id
                ORDER BY jp.posted_at DESC");
        }

        // Permanently deletes a job posting and applications
        public async Task DeleteJobPostingAsync(int jobId)
        {
            using var db = (MySqlConnection)Connection;

            await db.OpenAsync();

            using var tx = db.BeginTransaction();

            await db.ExecuteAsync(@"
                DELETE FROM applications
                WHERE job_id = @JobId",
                new { JobId = jobId }, tx);

            await db.ExecuteAsync(@"
                DELETE FROM job_postings
                WHERE job_id = @JobId",
                new { JobId = jobId }, tx);

            tx.Commit();
        }
    }
}