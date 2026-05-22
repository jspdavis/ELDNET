using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models.ViewModels
{
    /// <summary>
    /// ViewModel for submitting a job application (cover letter form).
    /// </summary>
    public class ApplicationViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        // Cover letter text is optional — user may upload a PDF instead.
        // If both are empty the submission is still valid (FR-20).
        [Display(Name = "Cover Letter")]
        public string CoverLetter { get; set; } = string.Empty;
    }
}
