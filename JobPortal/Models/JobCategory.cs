namespace JobPortal.Models
{
    /// <summary>
    /// Represents a job category (e.g., Engineering, Design, Marketing).
    /// Maps to the `job_categories` table.
    /// </summary>
    public class JobCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
