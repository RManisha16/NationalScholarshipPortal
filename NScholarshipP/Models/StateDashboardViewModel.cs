namespace NScholarshipP.Models
{
    public class StateDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int PendingStudents { get; set; }
        public int ForwardedCount { get; set; }
        public int TotalInstitutes { get; internal set; }
        public int PendingInstitutes { get; internal set; }
        public int ForwardedCountInstitute { get; internal set; }
    }
}
