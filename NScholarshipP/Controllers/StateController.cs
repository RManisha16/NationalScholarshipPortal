using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NScholarshipP.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NScholarshipP.Controllers
{
    public class StateController : Controller
    {
        private readonly ApplicationDbContext _db;

        // Seeded login credentials
        private const string StateUsername = "state";
        private const string StatePassword = "state123";

        public StateController(ApplicationDbContext db)
        {
            _db = db;
        }

        // LOGIN
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (username == StateUsername && password == StatePassword)
            {
                HttpContext.Session.SetString("IsState", "true");
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private bool IsState()
        {
            return HttpContext.Session.GetString("IsState") == "true";
        }

        // DASHBOARD
        public IActionResult Dashboard()
        {
            var totalStudents = _db.StudentApplications.Count();
            var pendingStudents = _db.StudentApplications
                .Count(s => s.Status == "Pending" || s.Status == "VerifiedByInstitute");
            var forwardedToMinistryStudents = _db.StudentApplications
                .Count(s => s.Status == "ForwardedToMinistry");
            var totalInstitutes = _db.InstituteApplications.Count();
            var pendingInstitutes = _db.InstituteApplications
                .Count(i => i.Status == "Pending" || i.Status == "VerifiedByState");
            var forwardedToMinistryInstitutes = _db.InstituteApplications
                .Where(i => i.Status == "ForwardedToMinistry").Count();
            var model = new StateDashboardViewModel
            {
                TotalStudents = totalStudents,
                PendingStudents = pendingStudents,
                ForwardedCount = forwardedToMinistryStudents,
                ForwardedCountInstitute = forwardedToMinistryInstitutes,
                TotalInstitutes = totalInstitutes,
                PendingInstitutes = pendingInstitutes
            };
            return View(model);
        }




        // STUDENT APPLICATIONS
        // show all relevant student applications including forwarded ones
        public IActionResult StudentApplications(string filter = "pending")
        {
            if (!IsState()) return RedirectToAction("Login");

            IQueryable<StudentApplication> query = _db.StudentApplications;

            switch (filter.ToLower())
            {
                case "pending":
                    query = query.Where(s => s.Status == "Pending" || s.Status == "VerifiedByInstitute");
                    break;

                case "approved":
                    query = query.Where(s => s.Status == "ApprovedByState" || s.Status == "ForwardedToMinistry");
                    break;

                case "rejected":
                    query = query.Where(s => s.Status == "RejectedByState");
                    break;

                case "all":
                default:
                    query = query.OrderByDescending(s => s.DateSubmitted);
                    break;
            }

            var list = query.OrderByDescending(s => s.DateSubmitted).ToList();

            ViewBag.CurrentFilter = filter;
            return View("StudentApplications", list);
        }


        // View details
        public IActionResult Details(int id)
        {
            if (id == null) return NotFound();

            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();
            return View(app); // Views/State/Details.cshtml
        }

        // Approve and forward to Ministry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveAndForward(int id)
        {
            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();

            app.Status = "ForwardedToMinistry";
            app.LastUpdatedOn = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "Application forwarded to Ministry.";
            return RedirectToAction("ForwardedApplications");
        }

        // Reject at state
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string? reason)
        {
            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();

            app.Status = "RejectedByState";
            app.AdminNotes = reason ?? "Rejected by State";
            app.LastUpdatedOn = DateTime.UtcNow;
            _db.SaveChanges();

            TempData["Success"] = "Application rejected.";
            return RedirectToAction("ForwardedApplications");
        }
    
        // forward single application to ministry (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardToMinistry(int id)
        {
            var app = await _db.StudentApplications.FindAsync(id);
            if (app == null)
            {
                TempData["Error"] = "Application not found.";
                return RedirectToAction("StudentApplications");
            }

            if (app.Status != "ApprovedByState")
            {
                TempData["Error"] = "Only approved applications can be forwarded.";
                return RedirectToAction("StudentApplications");
            }

            app.Status = "ForwardedToMinistry";
            _db.StudentApplications.Update(app);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Application of {app.StudentName} has been forwarded to Ministry.";
            return RedirectToAction("StudentApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var app = await _db.StudentApplications.FindAsync(id);
            if (app == null)
            {
                TempData["Error"] = "Application not found.";
                return RedirectToAction("StudentApplications");
            }

            app.Status = "ApprovedByState";
            _db.StudentApplications.Update(app);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Application of {app.StudentName} has been approved by state.";
            return RedirectToAction("StudentApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStudent(int id)
        {
            var app = await _db.StudentApplications.FindAsync(id);
            if (app == null)
            {
                TempData["Error"] = "Application not found.";
                return RedirectToAction("StudentApplications");
            }

            app.Status = "RejectedByState";
            _db.StudentApplications.Update(app);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Application of {app.StudentName} has been rejected by state.";
            return RedirectToAction("StudentApplications");
        }

        // INSTITUTE APPLICATIONS
        // list institute applications (pending)
        // List institute applications with filter
        public IActionResult InstituteApplications(string filter = "pending")
        {
            if (!IsState()) return RedirectToAction("Login");

            IQueryable<InstituteApplication> query = _db.InstituteApplications;

            switch (filter.ToLower())
            {
                case "pending":
                    // Only those that are not approved/forwarded/rejected yet
                    query = query.Where(i => i.Status == "Pending");
                    break;
                case "forwarded":
                    // Approved and forwarded to ministry
                    query = query.Where(i => i.Status == "VerifiedByState" || i.Status == "ForwardedToMinistry");
                    break;
                case "rejected":
                    query = query.Where(i => i.Status == "RejectedByState");
                    break;
                case "all":
                default:
                    // All applications
                    query = query.OrderByDescending(i => i.SubmittedOn);
                    break;
            }

            var list = query.OrderByDescending(i => i.SubmittedOn).ToList();

            ViewBag.CurrentFilter = filter;
            return View("InstituteApplications", list);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveInstitute(int id)
        {
            if (!IsState()) return RedirectToAction("Login", "Auth");
            var inst = _db.InstituteApplications.Find(id);
            if (inst == null) return RedirectToAction("InstituteApplications");
            inst.Status = "VerifiedByState";
            _db.SaveChanges();
            TempData["Success"] = "Institute approved by State.";
            return RedirectToAction("InstituteApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForwardInstituteToMinistry(int id)
        {
            if (!IsState()) return RedirectToAction("Login", "Auth");
            var inst = _db.InstituteApplications.Find(id);
            if (inst == null) return RedirectToAction("InstituteApplications");
            inst.Status = "ForwardedToMinistry";
            _db.SaveChanges();
            TempData["Success"] = "Institute forwarded to Ministry.";
            return RedirectToAction("InstituteApplications");
        }

        public IActionResult RejectInstitute(int id)
        {
            if (!IsState()) return RedirectToAction("Login");

            var app = _db.InstituteApplications.Find(id);
            if (app != null)
            {
                app.Status = "RejectedByState";
                _db.SaveChanges();
            }
            return RedirectToAction("InstituteApplications");
        }
        // ================= STUDENT DETAILS =================
        public IActionResult StudentDetails(int id)
        {
            if (!IsState()) return RedirectToAction("Login");

            var app = _db.StudentApplications.Find(id);
            if (app == null)
            {
                TempData["ErrorMessage"] = "Student application not found.";
                return RedirectToAction("StudentApplications");
            }

            return View( app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveStudentDetails(int id)
        {
            if (!IsState()) return RedirectToAction("Login");

            var app = _db.StudentApplications.Find(id);
            if (app != null)
            {
                app.Status = "ForwardedToMinistry";  // state approval forwards to ministry
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Application approved and forwarded to Ministry.";
            }

            return RedirectToAction("StudentApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectStudentDetails(int id)
        {
            if (!IsState()) return RedirectToAction("Login");

            var app = _db.StudentApplications.Find(id);
            if (app != null)
            {
                app.Status = "RejectedByState";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Application rejected.";
            }

            return RedirectToAction("StudentApplications");
        }



        // ================= INSTITUTE DETAILS =================
        public IActionResult InstituteDetails(int id)
        {
            if (!IsState()) return RedirectToAction("Login");

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
        public IActionResult ApproveInstituteDetails(int id)
        {
            if (!IsState()) return RedirectToAction("Login");

            var app = _db.InstituteApplications.Find(id);
            if (app != null)
            {
                app.Status = "ForwardedToMinistry";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Institute approved and forwarded to Ministry.";
            }

            return RedirectToAction("InstituteApplications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectInstituteDetails(int id)
        {
            if (!IsState()) return RedirectToAction("Login");

            var app = _db.InstituteApplications.Find(id);
            if (app != null)
            {
                app.Status = "RejectedByState";
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Institute application rejected.";
            }

            return RedirectToAction("InstituteApplications");
        }
    }
    public class StateViewModel
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
