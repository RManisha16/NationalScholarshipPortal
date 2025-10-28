using System.ComponentModel.DataAnnotations;
namespace NScholarshipP.Models
{
    public class StudentApplication
    {
        public int Id { get; set; }
        // Basic / Identity
        [Required] 
        public string Email { get; set; } = "";
        [Required] 
        public string StudentName { get; set; } = "";
        public DateTime? DateSubmitted { get; set; }
        public string Status { get; set; } = "Submitted";
        public string? SchemeName { get; set; }="";
        // Personal / Basic Details
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string AadharNumber { get; set; }
        // Academic Details
        public string? InstituteName { get; set; }
        public string? InstituteCode { get; set; }
        public string? PresentCourse { get; set; }
        public string? PresentCourseYear { get; set; }
        public string? UniversityOrBoardName { get; set; }
        public string? PreviousClassPercentage { get; set; }
        // 10th
        public string? X_RollNumber { get; set; }
        public string? X_BoardName { get; set; }
        public string? X_PassingYear { get; set; }
        public string? X_Percentage { get; set; }
        // 12th
        public string? XII_RollNumber { get; set; }
        public string? XII_BoardName { get; set; }
        public string? XII_PassingYear { get; set; }
        public string? XII_Percentage { get; set; }
        // Fee details
        public decimal? AdmissionFee { get; set; }
        public decimal? TuitionFee { get; set; }
        public decimal? OtherFee { get; set; }
        // Family
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string? GuardianName { get; set; }
        public decimal? FamilyAnnualIncome { get; set; }
        public string? ParentsProfession { get; set; }
        public int? NoOfSiblings { get; set; }

        // Disability / Other personal

        public bool? IsDisabled { get; set; }

        public string? DisabilityType { get; set; }

        public string? DisabilityPercent { get; set; }

        public string? MaritalStatus { get; set; }

        // Bank details

        public string? BankName { get; set; }

        public string? IFSCCode { get; set; }

        public string? BankAccount { get; set; }

        // Contact / Address

        public string? State { get; set; }

        public string? District { get; set; }

        public string? BlockOrTaluk { get; set; }

        public string? HouseNumber { get; set; }

        public string? StreetNumber { get; set; }

        public string? Pincode { get; set; }

        // Documents stored as relative paths (wwwroot/uploads/...)

        public string? PhotoPath { get; set; }

        public string? InstituteIdCardPath { get; set; }

        public string? CasteIncomeCertificatePath { get; set; }

        public string? PreviousMarksheetPath { get; set; }

        public string? FeeReceiptPath { get; set; }

        public string? BankPassbookPath { get; set; }

        public string? AadharPath { get; set; }

        public string? XMarksheetPath { get; set; }

        public string? XIIMarksheetPath { get; set; }

        public string? BonafideCertificatePath { get; set; } // for institute verification if needed

        public string? AdminNotes { get; internal set; }

        public DateTime? ApprovedOn { get; internal set; }

        public DateTime? LastUpdatedOn { get; internal set; }
        public int? SchemeId { get; set; }
        public int ScholarshipId { get; set; }
        public string Scholarship { get; set; }
        public string Course{ get; set; }
    }

}

