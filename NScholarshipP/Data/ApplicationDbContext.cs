using Microsoft.EntityFrameworkCore;
using NScholarshipP.Models;

namespace NScholarshipP.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<StudentApplication> StudentApplications { get; set; }
        public DbSet<InstituteApplication> InstituteApplications { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<ScholarshipScheme> ScholarshipSchemes { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

    }
}