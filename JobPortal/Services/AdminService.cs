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
                SELECT *
                FROM users
                WHERE role IN ('company', 'applicant')
                ORDER BY created_at DESC");
        }

        // Returns a single user by ID
        public async Task<User> GetUserByIdAsync(int userId)
        {
            using var db = Connection;

            return await db.QueryFirstOrDefaultAsync<User>(@"
                SELECT *
                FROM users
                WHERE user_id = @UserId",
                new { UserId = userId });
        }

        // Deletes a user if no linked data exists
        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var db = Connection;

            var applications = await db.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM applications
                WHERE applicant_id = @UserId",
                new { UserId = userId });

            var jobs = await db.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM job_postings
                WHERE company_id = @UserId",
                new { UserId = userId });

            if (applications > 0 || jobs > 0)
                return false;

            await db.ExecuteAsync(@"
                DELETE FROM users
                WHERE user_id = @UserId",
                new { UserId = userId });

            return true;
        }

        // Returns all categories
        public async Task<IEnumerable<JobCategory>> GetAllCategoriesAsync()
        {
            using var db = Connection;

            return await db.QueryAsync<JobCategory>(@"
                SELECT *
                FROM job_categories
                ORDER BY category_name");
        }

        // Creates a new category
        public async Task<int> CreateCategoryAsync(string categoryName)
        {
            using var db = Connection;

            var sql = @"
                INSERT INTO job_categories(category_name)
                VALUES(@CategoryName);

                SELECT LAST_INSERT_ID();";

            return await db.ExecuteScalarAsync<int>(
                sql,
                new { CategoryName = categoryName });
        }

        // Updates a category name
        public async Task UpdateCategoryAsync(
            int categoryId,
            string newName)
        {
            using var db = Connection;

            await db.ExecuteAsync(@"
                UPDATE job_categories
                SET category_name = @NewName
                WHERE category_id = @CategoryId",
                new
                {
                    CategoryId = categoryId,
                    NewName = newName
                });
        }

        // Deletes a category if unused
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            using var db = Connection;

            var jobs = await db.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM job_postings
                WHERE category_id = @CategoryId",
                new { CategoryId = categoryId });

            if (jobs > 0)
                return false;

            await db.ExecuteAsync(@"
                DELETE FROM job_categories
                WHERE category_id = @CategoryId",
                new { CategoryId = categoryId });

            return true;
        }

        // Returns all job postings
        public async Task<IEnumerable<JobPosting>> GetAllJobPostingsAsync()
        {
            using var db = Connection;

            return await db.QueryAsync<JobPosting>(@"
                SELECT jp.*
                FROM job_postings jp
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