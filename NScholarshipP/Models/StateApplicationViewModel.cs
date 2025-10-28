namespace NScholarshipP.Models
{
    public class StateApplicationViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string SchemeName { get; set; }
        public string InstituteName { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string Status { get; set; } // e.g., Pending, Approved, Rejected
        public bool IsForwardedToMinistry { get; set; } // true if forwarded
        public string MinistryStatus { get; set; } // e.g., Approved, Pending, Rejected
    }
}
