using Microsoft.AspNetCore.Hosting;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using NScholarshipP.Models;

using System;

using System.IO;

using System.Linq;

using System.Threading.Tasks;



namespace NScholarshipP.Controllers

{

    public class InstituteController : Controller

    {

        private readonly ApplicationDbContext _db;

        private readonly IWebHostEnvironment _env;

        private readonly PasswordHasher<InstituteApplication> _hasher;




        public InstituteController(ApplicationDbContext db, IWebHostEnvironment env)

        {

            _db = db;

            _env = env;

            _hasher = new PasswordHasher<InstituteApplication>();

        }



        // GET: Apply (Institute Application)

        public IActionResult Apply()

        {

            return View();

        }

        // POST: Apply (save application)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(InstituteApplication app, IFormFile establishCertificate, IFormFile affiliationCertificate)
        {
            if (!ModelState.IsValid)
            {
                // put diagnostics or return view with model => validation errors will show
                ViewBag.Diagnostics = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(app);
            }
            if (establishCertificate == null || establishCertificate.Length == 0
                || affiliationCertificate == null || affiliationCertificate.Length == 0)
            {
                ModelState.AddModelError("", "Please upload required certificates.");
                return View(app);
            }
            // Check for duplicate institute
            var duplicate = _db.InstituteApplications.Any(i =>
                i.InstituteName == app.InstituteName &&
                i.InstituteCode == app.InstituteCode);
            if (duplicate)
            {
                ModelState.AddModelError("", "An institute with the same Institute Name and Code already exists.");
                return View(app);
            }
            // Save files
            var uploadRoot = Path.Combine(_env.WebRootPath ?? ".", "uploads", "institutes");
            if (!Directory.Exists(uploadRoot)) Directory.CreateDirectory(uploadRoot);
            string SaveFile(IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName);
                var fname = $"{Guid.NewGuid()}{ext}";
                var full = Path.Combine(uploadRoot, fname);
                using var fs = new FileStream(full, FileMode.Create);
                file.CopyTo(fs);
                return $"/uploads/institutes/{fname}";
            }
            app.EstablishCertificatePath = SaveFile(establishCertificate);
            app.AffiliationCertificatePath = SaveFile(affiliationCertificate);
            // Hash the plain password provided in the transient property
            var hasher = new PasswordHasher<InstituteApplication>();
            app.PasswordHash = hasher.HashPassword(app, app.Password ?? string.Empty);
            app.Password = null;
            app.ConfirmPassword = null;
            app.SubmittedOn = DateTime.UtcNow;
            app.Status = "Pending";
            _db.InstituteApplications.Add(app);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Application submitted successfully.";
            return RedirectToAction("RegistrationConfirmation");
        }


        [HttpGet]
        public IActionResult RegistrationConfirmation()
        {
            return View();
        }

        // GET: /Institute/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Institute/Login

        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult Login(string instituteCode, string password)

        {

            // Find the institute

            var institute = _db.InstituteApplications

                .FirstOrDefault(i => i.InstituteCode == instituteCode && i.IsActiveLogin == true);



            if (institute == null)

            {

                ViewBag.Error = "Invalid institute code or account not activated yet.";

                return View();
            }
            // Verify hashed password
            var hasher = new PasswordHasher<InstituteApplication>();
            var result = hasher.VerifyHashedPassword(institute, institute.PasswordHash, password);
            if (result == PasswordVerificationResult.Success)
            {
                // Save session info
                HttpContext.Session.SetString("InstituteCode", institute.InstituteCode);
                HttpContext.Session.SetString("InstituteName", institute.InstituteName);
                // Redirect to Dashboard
                return RedirectToAction("Dashboard", "Institute");
            }
            // if wrong password
            ViewBag.Error = "Invalid password. Please try again.";
            return View();
        }



        // GET: /Institute/Dashboard

        [HttpGet]

        public IActionResult Dashboard()

        {

            var instituteCode = HttpContext.Session.GetString("InstituteCode");

            if (string.IsNullOrEmpty(instituteCode))

            {

                // not logged in

                return RedirectToAction("Login");

            }



            var institute = _db.InstituteApplications

                .FirstOrDefault(i => i.InstituteCode == instituteCode);



            ViewBag.InstituteName = institute?.InstituteName;

            ViewBag.Status = institute?.Status;

            ViewBag.InstituteCode = institute?.InstituteCode;

            ViewBag.DiseCode = institute?.DISECode;

            ViewBag.District = institute?.District;

            ViewBag.State = institute?.State;

            ViewBag.UniversityName = institute?.UniversityBoardName;

            ViewBag.YearAdmissionStarted = institute?.YearAdmissionStarted;

            ViewBag.Address = institute?.Address;

            ViewBag.PrincipalName = institute?.PrincipalName;

            ViewBag.Mobile = institute?.MobileNumber;

            //Add student statistics

            ViewBag.TotalStudents = _db.StudentApplications.Count(a => a.InstituteCode == instituteCode);

            ViewBag.PendingStudents = _db.StudentApplications.Count(a =>

                a.InstituteCode == instituteCode &&

                (a.Status == "Pending" || a.Status == "Submitted"));



            ViewBag.ForwardedStudents = _db.StudentApplications.Count(a =>

                a.InstituteCode == instituteCode &&

                a.Status == "VerifiedByInstitute");



            return View();

        }

        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult UpdateProfile(IFormCollection form)

        {

            var instituteCode = HttpContext.Session.GetString("InstituteCode");

            if (string.IsNullOrEmpty(instituteCode))

            {

                return RedirectToAction("Login");

            }



            var institute = _db.InstituteApplications.FirstOrDefault(i => i.InstituteCode == instituteCode);

            if (institute == null) return NotFound();



            //Safe parsing

            if (int.TryParse(form["YearAdmissionStarted"], out int year))

            {

                institute.YearAdmissionStarted = year;

            }

            else

            {

                institute.YearAdmissionStarted = null;

            }



            // Update other fields

            institute.DISECode = form["DISECode"];

            institute.District = form["District"];

            institute.State = form["State"];

            institute.UniversityBoardName = form["UniversityName"];

            institute.Address = form["Address"];

            institute.PrincipalName = form["PrincipalName"];

            institute.MobileNumber = form["MobileNumber"];



            _db.SaveChanges();



            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction("Dashboard");

        }





        // GET: Fetch student applications belonging to this institute

        // List applications that were forwarded to this institute by students

        // GET: /Institute/StudentApplications?instituteCode=ABC123

        public IActionResult StudentApplications()

        {

            var instituteCode = HttpContext.Session.GetString("InstituteCode");



            if (string.IsNullOrEmpty(instituteCode))

            {

                TempData["Error"] = "Institute not identified. Please log in.";

                return RedirectToAction("Login");

            }



            var applications = _db.StudentApplications

                 .Where(a => a.InstituteCode != null && a.InstituteCode.ToLower() == instituteCode.ToLower())

                 .OrderByDescending(a => a.DateSubmitted)

                 .ToList();



            return View(applications);

        }







        // GET: /Institute/Details/5
        public IActionResult Details(int id)
        {
            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();
            return View(app);
        }

        // POST: /Institute/Verify/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyStudent(int id)
        {
            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);
            if (app == null) return NotFound();
            // Example: set status and record verification time
            app.Status = "VerifiedByInstitute";
            app.AdminNotes = "Verified by institute on " + DateTime.Now.ToString("u");
            _db.SaveChanges();
            TempData["Success"] = "Application verified and forwarded to State.";
            return RedirectToAction("StudentApplications", new { instituteCode = app.InstituteCode });
        }



        // POST: /Institute/Reject/5

        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult Reject(int id, string reason)

        {

            var app = _db.StudentApplications.FirstOrDefault(a => a.Id == id);

            if (app == null) return NotFound();



            app.Status = "RejectedByInstitute";

            app.AdminNotes = reason ?? "Rejected by institute";

            _db.SaveChanges();



            TempData["Success"] = "Application rejected.";

            return RedirectToAction("StudentApplications", new { instituteCode = app.InstituteCode });

        }





        [HttpGet]

        public IActionResult ForgotPassword()

        {

            return View();

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult ForgotPassword(string InstituteCode)

        {

            var institute = _db.InstituteApplications.FirstOrDefault(i => i.InstituteCode == InstituteCode);

            if (institute == null)

            {

                TempData["Error"] = "Institute Code not found.";

                return View();

            }



            ViewBag.InstituteCode = InstituteCode;

            ViewBag.SecurityQuestion = institute.SecurityQuestion;

            return View("ResetPassword");

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult ResetPassword(string InstituteCode, string SecurityAnswer, string NewPassword, string ConfirmPassword)

        {

            var institute = _db.InstituteApplications.FirstOrDefault(i => i.InstituteCode == InstituteCode);

            if (institute == null || institute.SecurityAnswer != SecurityAnswer)

            {

                TempData["Error"] = "Invalid security answer.";

                ViewBag.InstituteCode = InstituteCode;

                return View();

            }



            if (NewPassword != ConfirmPassword)

            {

                TempData["Error"] = "Passwords do not match.";

                ViewBag.InstituteCode = InstituteCode;

                return View();

            }



            // Hash and save new password

            var hasher = new PasswordHasher<InstituteApplication>();

            institute.PasswordHash = hasher.HashPassword(institute, NewPassword);

            _db.SaveChanges();



            // Show success message on same page

            TempData["Success"] = "✅ Password reset successfully. Redirecting to login...";



            // Return view with redirect script

            ViewBag.InstituteCode = InstituteCode;

            ViewBag.RedirectToLogin = true;

            return View();

        }

        public IActionResult Logout()

        {

            HttpContext.Session.Remove("IsInstitute");

            HttpContext.Session.Remove("InstituteAppId");

            HttpContext.Session.Remove("InstituteName");

            HttpContext.Session.Clear();

            return RedirectToAction("Login");

        }

    }

}