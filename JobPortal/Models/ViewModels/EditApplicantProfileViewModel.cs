using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models.ViewModels
{
    public class EditApplicantProfileViewModel
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        [StringLength(2000)]
        public string Bio { get; set; }
    }
}
