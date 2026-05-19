using System;

namespace JobPortal.Models
{
    /// <summary>
    /// Represents an applicant's application for a job posting.
    /// Maps to the `applications` table.
    /// </summary>
    public class Application
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public int UserId { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";   // pending, reviewed, accepted, rejected
        public DateTime AppliedAt { get; set; }

        // Joined fields from related tables (not DB columns)
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
    }
}
