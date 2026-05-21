using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models.ViewModels
{
    /// <summary>
    /// ViewModel for creating or editing a job posting.
    /// Includes a dropdown list of categories.
    /// </summary>
    public class JobFormViewModel
    {
        public int JobId { get; set; } // 0 for Create, > 0 for Edit

        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        [Display(Name = "Job Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Requirements")]
        public string Requirements { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Salary range cannot exceed 100 characters.")]
        [Display(Name = "Salary Range")]
        public string SalaryRange { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Job Category")]
        public int CategoryId { get; set; }

        // Populated by the controller before rendering the form view
        public List<JobCategory> Categories { get; set; } = new List<JobCategory>();
    }
}
