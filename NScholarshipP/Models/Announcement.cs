using System.ComponentModel.DataAnnotations;

namespace NScholarshipP.Models
{
    public class Announcement
    {
        public int Id { get; set; }
        
        public required string Title { get; set; }
        public required string Message { get; set; }
        public DateTime DatePosted { get; set; }
        public bool IsPublic {  get; set; }
    }
}
