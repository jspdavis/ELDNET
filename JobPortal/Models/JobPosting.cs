using System;

namespace JobPortal.Models
{
    /// <summary>
    /// Represents a job posting created by a company.
    /// Maps to the `job_postings` table.
    /// </summary>
    public class JobPosting
    {
        public int JobId { get; set; }
        public int CompanyId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string SalaryRange { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime PostedAt { get; set; }

        // Joined fields from related tables (not DB columns)
        public string CompanyName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int ApplicationCount { get; set; }
    }
}
