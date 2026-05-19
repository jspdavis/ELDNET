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
        public string CompanyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        // Joined from users table (not a DB column)
        public string OwnerEmail { get; set; } = string.Empty;
        public string OwnerFullName { get; set; } = string.Empty;
    }
}
