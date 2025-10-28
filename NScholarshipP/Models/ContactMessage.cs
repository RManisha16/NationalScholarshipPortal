using System;
using System.ComponentModel.DataAnnotations;
namespace NScholarshipP.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your name")]

        [StringLength(100)]

        [Display(Name = "Full name")]

        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email")]

        [EmailAddress(ErrorMessage = "Enter a valid email")]

        public string Email { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number")]

        [Display(Name = "Mobile number")]

        public string Phone { get; set; }

        [Required(ErrorMessage = "Please enter a subject")]

        [StringLength(150)]

        public string Subject { get; set; }

        [Required(ErrorMessage = "Please enter a message")]

        [StringLength(2000)]

        [DataType(DataType.MultilineText)]

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;


    }

}

