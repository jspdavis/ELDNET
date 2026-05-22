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

        // Resume file fields — mapped from applicant_profiles columns
        public string ResumeFilename { get; set; }      // server-side stored UUID filename
        public string ResumeOriginalName { get; set; }  // original filename the user uploaded
        public DateTime? ResumeUploadedAt { get; set; } // when the resume was uploaded
    }
}
