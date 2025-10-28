using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NScholarshipP.Models
{
    public class InstituteApplication
    {
        public int Id { get; set; }
        // Basic details / application
        [Required]
        public string InstituteName { get; set; } = string.Empty;
        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;
        // Unique identifiers (app asks for these)
        [Required]
        public string InstituteCode { get; set; } = string.Empty;  // username for login
        [Required]
        public string DISECode { get; set; } = string.Empty;
        // We store the hashed password in the application. When ministry approves,
        // the institute can log in using the same InstituteCode + password.
        public string PasswordHash { get; set; } = string.Empty;
        [NotMapped]
        [Required]//important: this means it won't be stored in DB
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        [Required]//important: this means it won't be stored in DB
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
        // Location, type, affiliation
        [Required]
        public string Location { get; set; } // "Rural" or "Urban"
        [Required]
        public string InstituteType { get; set; } // e.g. "College", "School"
        [Required]
        public string AffiliatedUniversityState { get; set; }
        [Required]
        public string UniversityBoardName { get; set; }
        [Required]
        public int? YearAdmissionStarted { get; set; }
        // Address & contact
        public string Address { get; set; }
        public string PrincipalName { get; set; }
        public string MobileNumber { get; set; }
        public string? Telephone { get; set; }
        // Document paths (relative URLs under wwwroot/uploads/institutes/)
        public string EstablishCertificatePath { get; set; } = string.Empty;
        public string AffiliationCertificatePath { get; set; } = string.Empty;
        public bool DeclarationAccepted { get; set; } = false;
        // Application tracking
        public string Status { get; set; } = "Pending"; // Pending, VerifiedByState, ForwardedToMinistry, ApprovedByMinistry, RejectedByState, RejectedByMinistry
                                                        // public string DocumentsPath { get; set; }
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedOn { get; set; }
        // When ministry approves, set to true so institute can login
        public bool IsActiveLogin { get; set; } = false;
        public string? AdminNotes { get; set; }
        public DateTime LastUpdatedOn { get; internal set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
    }
}
