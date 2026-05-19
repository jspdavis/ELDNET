using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Registration form.
    /// Supports both applicant and company roles.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an account type.")]
        [Display(Name = "I am registering as a...")]
        public string Role { get; set; } = "applicant"; // "company" or "applicant"

        // --- Company-only fields (shown/required when Role == "company") ---

        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Company Description")]
        public string? CompanyDescription { get; set; }

        [Display(Name = "Company Location")]
        public string? CompanyLocation { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL (e.g. https://company.com).")]
        [Display(Name = "Company Website")]
        public string? CompanyWebsite { get; set; }
    }
}
