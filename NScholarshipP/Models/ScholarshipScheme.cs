namespace NScholarshipP.Models
{
    public class ScholarshipScheme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string EligibilityCriteria { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public string RequiredDocuments { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
    }
}