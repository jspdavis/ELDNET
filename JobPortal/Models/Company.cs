using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    /// <summary>
    /// Extended profile for users with role = 'company'.
    /// Maps to the `companies` table and may be joined with `users`.
    /// </summary>
    public class Company
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
        public string CompanyName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;

        [Url(ErrorMessage = "Please enter a valid URL (e.g. https://company.com).")]
        [MaxLength(255, ErrorMessage = "Website URL cannot exceed 255 characters.")]
        public string Website { get; set; } = string.Empty;

        // Joined from users table (not a DB column)
        public string OwnerEmail { get; set; } = string.Empty;
        public string OwnerFullName { get; set; } = string.Empty;
    }
}
