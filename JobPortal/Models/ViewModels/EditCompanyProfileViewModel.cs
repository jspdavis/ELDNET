using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models.ViewModels
{
    public class EditCompanyProfileViewModel
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; }         // contact person name

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        [Url]
        [StringLength(500)]
        public string Website { get; set; }
    }
}
