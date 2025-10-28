using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NScholarshipP.Models
{
    public class Student
    {
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        // Persistent hashed password (stored in DB)
        public string PasswordHash { get; set; } = string.Empty;
        // Security Q/A, etc.
        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }
        public string? MobileNumber { get; set; }
        public string? PhotoPath { get; set; }

        [NotMapped]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [NotMapped]
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

