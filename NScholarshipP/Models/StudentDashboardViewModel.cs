namespace NScholarshipP.Models
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }

        public string StudentEmail { get; set; }

        public string PhotoPath { get; set; }

        public List<string> Schemes { get; set; } = new();

        public int TotalApplications { get; set; }

        public int ApprovedCount { get; set; }

        public int PendingCount { get; set; }

    }

}

