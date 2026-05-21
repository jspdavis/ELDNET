using System;
using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required.")]
        public string Description { get; set; } = string.Empty;

        public string Requirements { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Salary range cannot exceed 100 characters.")]
        public string SalaryRange { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime PostedAt { get; set; }

        // Joined fields from related tables (not DB columns)
        public string CompanyName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int ApplicationCount { get; set; }
    }
}
