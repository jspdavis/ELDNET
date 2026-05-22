using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    /// <summary>
    /// Represents a user account (either 'company' or 'applicant').
    /// Maps directly to the `users` table in the database.
    /// </summary>
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(company|applicant|admin)$", ErrorMessage = "Role must be either 'company', 'applicant', or 'admin'.")]
        public string Role { get; set; } = string.Empty;   // "company", "applicant", or "admin"

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
