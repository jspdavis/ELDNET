using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class CompanyProfile
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }     // contact person, from users.full_name
        public string Email { get; set; }        // from users.email
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Website { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
