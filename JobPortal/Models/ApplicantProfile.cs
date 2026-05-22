using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class ApplicantProfile
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }   // joined from users.full_name
        public string Email { get; set; }      // joined from users.email
        public string Phone { get; set; }
        public string Location { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
