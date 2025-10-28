using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NScholarshipP.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NScholarshipP.Controllers
{
    public class MinistryController : Controller
    {
        private readonly ApplicationDbContext _db;

        // Seeded login credentials
        private const string MinistryUsername = "ministry";
        private const string MinistryPassword = "ministry123";

        public MinistryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ======================= LOGIN ===========================
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (username == MinistryUsername && password == MinistryPassword)
            {
                HttpContext.Session.SetString("IsMinistry", "true");
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsMinistry");
            return RedirectToAction("Login");
        }

        private bool IsMinistry()
        {
            return HttpContext.Session.GetString("IsMinistry") == "true";
        }

        // ======================= DASHBOARD ===========================
        public async Task<IActionResult> Dashboard()
        {
            if (!IsMinistry())
                return RedirectToAction("Login");

            var totalStudents = _db.StudentApplications.Count();
            var totalInstitutes = _db.InstituteApplications.Count();
            var pendingStudents = _db.StudentApplications.Count(a => a.Status == "Pending" || a.Status == "VerifiedByInstitute" || a.Status == "ForwardedToMinistry");
            var pendingInstitutes = _db.InstituteApplications.Count(a => a.Status =="Pending" || a.Status == "VerifiedByState" || a.Status == "ForwardedToMinistry" );


            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalInstitutes = totalInstitutes;
            ViewBag.PendingStudents = pendingStudents;
            ViewBag.PendingInstitutes = pendingInstitutes;


            return View();
        }

        // ======================= STUDENT APPLICATIONS ===========================
        public async Task<IActionResult> StudentApplications(string filter = "Pending")
        {
            if (!IsMinistry()) return RedirectToAction("Login");

            filter = (filter ?? "Pending").ToLower(); // normalize filter
            IQueryable<StudentApplication> applications = _db.StudentApplications;

            switch (filter)
            {
                case "pending":
                    // Only show applications that are forwarded to ministry or still pending
                    applications = applications.Where(a => a.Status == "Pending" || a.Status == "ForwardedToMinistry");
                    break;

                case "approved":
                    // Only show applications approved by ministry
                    applications = applications.Where(a => a.Status == "Approved" || a.Status == "ApprovedByMinistry");
                    break;

                case "rejected":
                    // Any rejected applications
                    applications = applications.Where(a => a.Status.Contains("Rejected"));
                    break;

                case "all":
                default:
                    // Show all applications
                    break;
            }

            var list = await applications
                .OrderByDescending(a => a.DateSubmitted)
                .ToListAsync();

            ViewBag.CurrentFilter = filter;
            return View(list); // Views/Ministry/StudentApplications.cshtml
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var student = await _db.StudentApplications.FindAsync(id);
            if (student == null)
            {
                TempData["Error"] = "Student application not found.";
                return RedirectToAction("StudentApplications");
            }

            student.Status = "Approved";
            student.LastUpdatedOn = DateTime.Now;
            _db.Update(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Student application approved successfully.";
            return RedirectToAction("StudentApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStudent(int id)
        {
            var student = await _db.StudentApplications.FindAsync(id);
            if (student == null)
            {
                TempData["Error"] = "Student application not found.";
                return RedirectToAction("StudentApplications");
            }

            student.Status = "RejectedByMinistry";
            student.LastUpdatedOn = DateTime.Now;
            _db.Update(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Student application rejected successfully.";
            return RedirectToAction("StudentApplications");
        }
        // Applications forwarded to ministry
        public IActionResult ToReview()
        {
            var list = _db.StudentApplications
                          .Where(a => a.Status == "ForwardedToMinistry")
                          .OrderByDescending(a => a.DateSubmitted)
                          .ToList();
            return View(list); // Views/Ministry/ToReview.cshtml
        }

        // Detail view
        public IActionResult Details(int id)
        {
            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();
            return View(app); // Views/Ministry/Details.cshtml
        }

        // Final approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();

            app.Status = "Approved";
            app.ApprovedOn = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "Application approved.";
            return RedirectToAction("ToReview");
        }

        // Reject at ministry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string? reason)
        {
            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();

            app.Status = "RejectedByMinistry";
            app.AdminNotes = reason ?? "Rejected by Ministry";
            app.LastUpdatedOn = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "Application rejected by ministry.";
            return RedirectToAction("ToReview");
        }
        // ======================= INSTITUTE APPLICATIONS ===========================
        public async Task<IActionResult> InstituteApplications(string filter)
        {
            ViewBag.CurrentFilter = filter ?? "All";

            var applications = _db.InstituteApplications.AsQueryable();

            switch (filter)
            {
                case "Pending":
                    applications = applications.Where(i => i.Status == "Pending" || i.Status == "VerifiedByState" || i.Status == "ForwardedToMinistry");
                    break;
                case "Approved":
                    applications = applications.Where(i => i.Status.Contains("Approved"));
                    break;
                case "Rejected":
                    applications = applications.Where(i => i.Status.Contains("Rejected"));
                    break;
                case "All":
                default:
                    break;
            }

            var list = await applications
                .OrderByDescending(i => i.SubmittedOn)
                .ToListAsync();

            return View(list);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveInstituteByMinistry(int id)
        {
            var institute = _db.InstituteApplications.FirstOrDefault(i => i.Id == id);
            if (institute == null)
            {
                TempData["Error"] = "Institute not found.";
                return RedirectToAction("InstituteApplications");
            }

            institute.Status = "ApprovedByMinistry";
            institute.LastUpdatedOn = DateTime.Now;
            institute.IsActiveLogin= true; // Enable login

            _db.Update(institute);
            _db.SaveChanges();

            TempData["Success"] = "Institute approved successfully and can now log in.";
            return RedirectToAction("InstituteDetails", new { id });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInstitute(int id, string? reason)
        {
            var institute = await _db.InstituteApplications.FindAsync(id);
            if (institute == null)
            {
                TempData["Error"] = "Institute not found.";
                return RedirectToAction("InstituteApplications");
            }

            institute.Status = "RejectedByMinistry";
            institute.LastUpdatedOn = DateTime.Now;
            institute.AdminNotes = reason ?? "Rejected by Ministry";
            _db.Update(institute);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Institute rejected successfully.";
            return RedirectToAction("InstituteDetails", new { id });
        }


        // GET: /Ministry/StudentDetails/5
        public async Task<IActionResult> StudentDetails(int id)
        {
            var student = await _db.StudentApplications.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StudentDetailsApprove(int id)
        {
            if (!IsMinistry()) return RedirectToAction("Login", "Ministry");

            var app = _db.StudentApplications.Find(id);
            if (app != null)
            {
                app.Status = "Approved";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Student application approved.";
            }
            return RedirectToAction("StudentApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StudentDetailsReject(int id)
        {
            if (!IsMinistry()) return RedirectToAction("Login", "Ministry");

            var app = _db.StudentApplications.Find(id);
            if (app != null)
            {
                app.Status = "Rejected";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Student application rejected.";
            }
            return RedirectToAction("StudentApplications");
        }
        // GET: /Ministry/InstituteDetails/5
        public IActionResult InstituteDetails(int id)
        {
            if (!IsMinistry()) return RedirectToAction("Login", "Ministry");

            var app = _db.InstituteApplications.Find(id);
            if (app == null)
            {
                TempData["ErrorMessage"] = "Institute application not found.";
                return RedirectToAction("InstituteApplications");
            }
            return View("InstituteDetails", app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InstituteDetailsApprove(int id)
        {
            if (!IsMinistry()) return RedirectToAction("Login", "Ministry");

            var app = _db.InstituteApplications.Find(id);
            if (app != null)
            {
                app.Status = "Approved";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Institute approved.";
            }
            return RedirectToAction("InstituteApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InstituteDetailsReject(int id)
        {
            if (!IsMinistry()) return RedirectToAction("Login", "Ministry");

            var app = _db.InstituteApplications.Find(id);
            if (app != null)
            {
                app.Status = "Rejected";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Institute rejected.";
            }
            return RedirectToAction("InstituteApplications");
        }
    }
    public class MinistryViewModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
