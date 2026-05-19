using Dapper;
using JobPortal.Data;
using JobPortal.Models;

namespace JobPortal.Services
{
    /// <summary>
    /// Handles all job posting database operations using Dapper.
    /// </summary>
    public class JobService : IJobService
    {
        private readonly DatabaseContext _context;

        public JobService(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all active job postings joined with company and category info.
        /// Used on the public jobs listing page.
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetAllActiveJobsAsync()
        {
            using var db = _context.CreateConnection();

            // Get all jobs where is_active = 1, newest first
            const string sql = @"
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
                       cat.category_name AS CategoryName,
                       COUNT(a.application_id) AS ApplicationCount
                FROM job_postings jp
                INNER JOIN companies     c   ON c.company_id    = jp.company_id
                INNER JOIN job_categories cat ON cat.category_id = jp.category_id
                LEFT  JOIN applications  a   ON a.job_id        = jp.job_id
                WHERE jp.is_active = 1
                GROUP BY jp.job_id, jp.company_id, jp.category_id, jp.title, jp.description,
                         jp.requirements, jp.location, jp.salary_range, jp.is_active, jp.posted_at,
                         c.company_name, cat.category_name
                ORDER BY jp.posted_at DESC";

            return await db.QueryAsync<JobPosting>(sql);
        }

        /// <summary>
        /// Returns active jobs filtered by keyword (searches title and description)
        /// and/or category. Used by the search bar and AJAX category filter.
        /// </summary>
        public async Task<IEnumerable<JobPosting>> SearchJobsAsync(string? keyword, int? categoryId)
        {
            using var db = _context.CreateConnection();

            // Build WHERE clause dynamically based on provided filters
            const string sql = @"
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
                       cat.category_name AS CategoryName,
                       COUNT(a.application_id) AS ApplicationCount
                FROM job_postings jp
                INNER JOIN companies     c   ON c.company_id    = jp.company_id
                INNER JOIN job_categories cat ON cat.category_id = jp.category_id
                LEFT  JOIN applications  a   ON a.job_id        = jp.job_id
                WHERE jp.is_active = 1
                  AND (@Keyword    IS NULL OR jp.title LIKE CONCAT('%', @Keyword, '%')
                                          OR jp.description LIKE CONCAT('%', @Keyword, '%'))
                  AND (@CategoryId IS NULL OR jp.category_id = @CategoryId)
                GROUP BY jp.job_id, jp.company_id, jp.category_id, jp.title, jp.description,
                         jp.requirements, jp.location, jp.salary_range, jp.is_active, jp.posted_at,
                         c.company_name, cat.category_name
                ORDER BY jp.posted_at DESC";

            return await db.QueryAsync<JobPosting>(sql, new
            {
                Keyword    = string.IsNullOrWhiteSpace(keyword) ? null : keyword,
                CategoryId = categoryId
            });
        }

        /// <summary>
        /// Returns full details for a single job posting by ID.
        /// Returns null if the job does not exist or is not active.
        /// </summary>
        public async Task<JobPosting?> GetJobByIdAsync(int jobId)
        {
            using var db = _context.CreateConnection();

            // Fetch job with company and category info; allow inactive (for company edit page)
            const string sql = @"
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
                       cat.category_name AS CategoryName
                FROM job_postings jp
                INNER JOIN companies     c   ON c.company_id    = jp.company_id
                INNER JOIN job_categories cat ON cat.category_id = jp.category_id
                WHERE jp.job_id = @JobId
                LIMIT 1";

            return await db.QueryFirstOrDefaultAsync<JobPosting>(sql, new { JobId = jobId });
        }

        /// <summary>
        /// Returns all job postings (active and inactive) for a specific company.
        /// Used on the Company Dashboard.
        /// </summary>
        public async Task<IEnumerable<JobPosting>> GetJobsByCompanyAsync(int companyId)
        {
            using var db = _context.CreateConnection();

            // Return all jobs for this company, including applicant count per job
            const string sql = @"
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
                       cat.category_name AS CategoryName,
                       COUNT(a.application_id) AS ApplicationCount
                FROM job_postings jp
                INNER JOIN companies     c   ON c.company_id    = jp.company_id
                INNER JOIN job_categories cat ON cat.category_id = jp.category_id
                LEFT  JOIN applications  a   ON a.job_id        = jp.job_id
                WHERE jp.company_id = @CompanyId
                GROUP BY jp.job_id, jp.company_id, jp.category_id, jp.title, jp.description,
                         jp.requirements, jp.location, jp.salary_range, jp.is_active, jp.posted_at,
                         c.company_name, cat.category_name
                ORDER BY jp.posted_at DESC";

            return await db.QueryAsync<JobPosting>(sql, new { CompanyId = companyId });
        }

        /// <summary>
        /// Inserts a new job posting into the database.
        /// Returns the new auto-incremented job_id.
        /// </summary>
        public async Task<int> CreateJobAsync(int companyId, int categoryId, string title,
            string description, string requirements, string location, string salaryRange)
        {
            using var db = _context.CreateConnection();

            // Insert and return the new job ID
            const string sql = @"
                INSERT INTO job_postings
                    (company_id, category_id, title, description, requirements, location, salary_range, is_active, posted_at)
                VALUES
                    (@CompanyId, @CategoryId, @Title, @Description, @Requirements, @Location, @SalaryRange, 1, NOW());
                SELECT LAST_INSERT_ID();";

            return await db.ExecuteScalarAsync<int>(sql, new
            {
                CompanyId    = companyId,
                CategoryId   = categoryId,
                Title        = title,
                Description  = description,
                Requirements = requirements,
                Location     = location,
                SalaryRange  = salaryRange
            });
        }

        /// <summary>
        /// Updates an existing job posting's editable fields.
        /// Only the owning company should be allowed to call this.
        /// </summary>
        public async Task UpdateJobAsync(int jobId, int categoryId, string title,
            string description, string requirements, string location, string salaryRange)
        {
            using var db = _context.CreateConnection();

            // Update all editable fields for the given job_id
            const string sql = @"
                UPDATE job_postings
                SET category_id  = @CategoryId,
                    title        = @Title,
                    description  = @Description,
                    requirements = @Requirements,
                    location     = @Location,
                    salary_range = @SalaryRange
                WHERE job_id = @JobId";

            await db.ExecuteAsync(sql, new
            {
                JobId        = jobId,
                CategoryId   = categoryId,
                Title        = title,
                Description  = description,
                Requirements = requirements,
                Location     = location,
                SalaryRange  = salaryRange
            });
        }

        /// <summary>
        /// Soft-deletes a job by setting is_active = 0.
        /// The record is retained in the database for historical purposes.
        /// </summary>
        public async Task DeleteJobAsync(int jobId)
        {
            using var db = _context.CreateConnection();

            // Mark the job as inactive instead of physically deleting it
            const string sql = "UPDATE job_postings SET is_active = 0 WHERE job_id = @JobId";

            await db.ExecuteAsync(sql, new { JobId = jobId });
        }

        /// <summary>
        /// Returns all job categories from the lookup table.
        /// Used to populate category dropdowns and filter buttons.
        /// </summary>
        public async Task<IEnumerable<JobCategory>> GetAllCategoriesAsync()
        {
            using var db = _context.CreateConnection();

            // Simple select of all categories ordered alphabetically
            const string sql = @"
                SELECT category_id   AS CategoryId,
                       category_name AS CategoryName
                FROM job_categories
                ORDER BY category_name ASC";

            return await db.QueryAsync<JobCategory>(sql);
        }
    }
}
