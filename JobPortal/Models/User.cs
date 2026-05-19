using System;

namespace JobPortal.Models
{
    /// <summary>
    /// Represents a user account (either 'company' or 'applicant').
    /// Maps directly to the `users` table in the database.
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;   // "company" or "applicant"
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
