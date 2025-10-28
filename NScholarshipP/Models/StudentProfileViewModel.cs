namespace NScholarshipP.Models

{

    public class StudentProfileViewModel

    {

        public string FullName { get; set; }

        public string Email { get; set; }

        public string PhotoPath { get; set; }

        public int TotalApplications { get; set; }

        public int ApprovedCount { get; set; }

        public int PendingCount { get; set; }

        public DateTime? LastApplicationDate { get; set; }

    }

}

