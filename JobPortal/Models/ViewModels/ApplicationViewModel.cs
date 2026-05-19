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

        [Required(ErrorMessage = "A cover letter is required.")]
        [MinLength(50, ErrorMessage = "Cover letter must be at least 50 characters.")]
        [Display(Name = "Cover Letter")]
        public string CoverLetter { get; set; } = string.Empty;
    }
}
