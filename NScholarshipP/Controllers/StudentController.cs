using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NScholarshipP.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class StudentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly PasswordHasher<Student> _passwordHasher;

    public StudentController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
        _passwordHasher = new PasswordHasher<Student>();
    }

    // ----------------- Register -----------------
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Student model)
    {
        // Basic server-side checks
        if (model == null)
        {
            ModelState.AddModelError("", "Invalid form submission.");
            return View(model);
        }
        if (!ModelState.IsValid)
        {
            // collect validation errors for quick debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .Where(m => !string.IsNullOrWhiteSpace(m))
                                          .ToList();
            TempData["Error"] = "Validation failed: " + (errors.Any() ? string.Join(" | ", errors) : "see fields.");
            return View(model);
        }

        try
        {
            // ensure email uniqueness
            var exists = await _db.Students.FirstOrDefaultAsync(s => s.Email == model.Email);
            if (exists != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already registered.");
                TempData["Error"] = "Email already registered.";
                return View(model);
            }

            // hash password - requires Microsoft.AspNetCore.Identity
            var hasher = new PasswordHasher<Student>();
            model.PasswordHash = hasher.HashPassword(model, model.Password);

            // clear plain password fields so they are not accidentally used/stored
            model.Password = null;
            model.ConfirmPassword = null;

            // add and save
            _db.Students.Add(model);
            await _db.SaveChangesAsync();

            // set session
            HttpContext.Session.SetString("StudentEmail", model.Email);
            HttpContext.Session.SetString("StudentName", model.FullName);
            HttpContext.Session.SetString("StudentPhotoPath", model.PhotoPath ?? "/images/default-profile.png");

            TempData["Success"] = "Registration successful.";
            return RedirectToAction("Dashboard", "Student");
        }
        catch (Exception ex)
        {
            // show the deepest inner exception to TempData so you can paste it here
            var inner = ex.GetBaseException().Message;
            TempData["Error"] = "Error saving user: " + inner;
            // also add modelstate-level message
            ModelState.AddModelError("", "Error saving user: " + inner);
            return View(model);
        }
    }

    // ----------------- Login -----------------
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password, Student model)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Please enter both email and password.";
            return View();
        }

        try
        {
            var student = _db.Students.FirstOrDefault(s => s.Email == email);

            if (student == null)
            {
                ViewBag.Error = "No student found with that email.";
                return View();
            }

            if (string.IsNullOrEmpty(student.PasswordHash))
            {
                ViewBag.Error = "PasswordHash is empty. Please re-register the student.";
                return View();
            }

            // Debug check — see what is stored
            Console.WriteLine($"Stored hash: {student.PasswordHash}");

            var result = new PasswordHasher<Student>()
                .VerifyHashedPassword(student, student.PasswordHash, password);

            Console.WriteLine($"Verify result: {result}");

            if (result == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetString("StudentEmail", student.Email);
                HttpContext.Session.SetString("StudentName", student.FullName);
                HttpContext.Session.SetString("StudentPhotoPath", student.PhotoPath ?? "/images/default-profile.png");

                TempData["Success"] = "Login successful.";
                return RedirectToAction("Dashboard", "Student");
            }
            else
            {
                ViewBag.Error = "Password does not match.";
                return View();
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Exception: " + ex.GetBaseException().Message;
            return View();
        }
    }

    // ----------------- Logout -----------------
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("StudentId");
        HttpContext.Session.Remove("StudentEmail");
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    // ----------------- Dashboard -----------------
    public IActionResult Dashboard()
    {
        var email = HttpContext.Session.GetString("StudentEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Student");
        var all = _db.StudentApplications.Where(a => a.Email == email).ToList();
        var model = new StudentDashboardViewModel
        {
            StudentName = HttpContext.Session.GetString("StudentName"),
            StudentEmail = email,
            PhotoPath = HttpContext.Session.GetString("StudentPhotoPath"),
            Schemes = new List<string>
    {
        "Post Matric Scholarship",
        "National Merit Scholarship",
        "Central Scholarship",
        "Pragati Scholarship",
        "Merit Based Scholarship (NTSE)"
    },
            TotalApplications = all.Count,
            ApprovedCount = all.Count(a => a.Status == "Approved"),
            PendingCount = all.Count(a => a.Status == "Pending" || a.Status == "ForwardedToInstitute" || a.Status == "ForwardedToState")
        };
        return View(model);
    }





    // GET: Apply Page

    [HttpGet]
    public IActionResult Apply(string scheme)
    {
        var email = HttpContext.Session.GetString("StudentEmail");

        if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Student");

        var model = new StudentApplication

        {

            SchemeName = scheme,
            Scholarship = scheme,

            Email = email,

            StudentName = HttpContext.Session.GetString("StudentName")

        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(
    StudentApplication model,
    IFormFile Photo,
    IFormFile InstituteIdCard,
    IFormFile CasteIncomeCertificate,
    IFormFile PreviousMarksheet,
    IFormFile FeeReceipt,
    IFormFile BankPassbook,
    IFormFile AadharCard,
    IFormFile XMarksheet,
    IFormFile XIIMarksheet)
    {
        Console.WriteLine("---- APPLY POST STARTED ----");

        if (!ModelState.IsValid)
        {
            var errs = ModelState
                .Where(kv => kv.Value.Errors.Count > 0)
                .Select(kv => kv.Key + " => " + string.Join(", ", kv.Value.Errors.Select(e => e.ErrorMessage)))
                .ToList();
            TempData["Error"] = "Model invalid: " + string.Join(" | ", errs);
            Console.WriteLine(TempData["Error"]);
            return View(model);
        }

        try
        {
            var studentFolder = Path.Combine(_env.WebRootPath, "uploads", "students"); // ✅ fixed
            if (!Directory.Exists(studentFolder))
            {
                Directory.CreateDirectory(studentFolder);
                Console.WriteLine("Created uploads/students folder");
            }

            string SaveFile(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return null;

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowed.Contains(ext))
                    return null;

                var fileName = Guid.NewGuid().ToString("N") + ext;
                var path = Path.Combine(studentFolder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return "/uploads/students/" + fileName;
            }

            // Fix file saving
            model.PhotoPath = SaveFile(Photo);
            model.InstituteIdCardPath = SaveFile(InstituteIdCard);
            model.CasteIncomeCertificatePath = SaveFile(CasteIncomeCertificate);
            model.PreviousMarksheetPath = SaveFile(PreviousMarksheet);
            model.FeeReceiptPath = SaveFile(FeeReceipt);
            model.BankPassbookPath = SaveFile(BankPassbook);
            model.AadharPath = SaveFile(AadharCard);
            model.XMarksheetPath = SaveFile(XMarksheet);
            model.XIIMarksheetPath = SaveFile(XIIMarksheet);

            // Populate other fields
            model.Email = HttpContext.Session.GetString("StudentEmail") ?? model.Email;
            model.StudentName = HttpContext.Session.GetString("StudentName") ?? model.StudentName;
            model.DateSubmitted = DateTime.Now;
            model.Status = "Submitted";

            _db.StudentApplications.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Application submitted successfully!";
            Console.WriteLine("Application submitted successfully.");
            return RedirectToAction("MyApplications");
        }
        catch (Exception ex)
        {
            var err = ex.GetBaseException().Message;
            TempData["Error"] = "Error while saving: " + err;
            Console.WriteLine("Exception: " + err);
            return View(model);
        }
    }

    // ----------------- My Applications (list) -----------------
    // GET: Student/MyApplications
    public async Task<IActionResult> MyApplications()
    {
        // assume student's identity is tracked via session/email
        var email = HttpContext.Session.GetString("StudentEmail");
        if (string.IsNullOrEmpty(email))
        {
            // optionally redirect to login
            TempData["Error"] = "Please login to view your applications.";
            return RedirectToAction("Login");
        }

        var apps = await _db.StudentApplications
                        .Where(a => a.Email == email)
                        .OrderByDescending(a => a.DateSubmitted)
                        .ToListAsync();

        return View(apps); // expects List<StudentApplication>
    }

    // GET: Student/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var app = await _db.StudentApplications
                        .FirstOrDefaultAsync(a => a.Id == id.Value);

        if (app == null)
            return NotFound();
        var email = HttpContext.Session.GetString("StudentEmail");
        if (!string.IsNullOrEmpty(email) && app.Email != email)
        {
            // forbid viewing another student's application
            return Forbid();
        }

        return View(app); // expects StudentApplication model
    }

    // GET: /Student/Profile
    public IActionResult Profile()
    {
        var email = HttpContext.Session.GetString("StudentEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Student");

        var studentApps = _db.StudentApplications
            .Where(a => a.Email == email)
            .ToList();

        var vm = new StudentProfileViewModel
        {
            FullName = HttpContext.Session.GetString("StudentName"),
            Email = email,
            PhotoPath = HttpContext.Session.GetString("StudentPhotoPath"),
            TotalApplications = studentApps.Count,
            ApprovedCount = studentApps.Count(a => a.Status == "Approved"),
            PendingCount = studentApps.Count(a => a.Status != "Approved" && a.Status != "Rejected"),
            LastApplicationDate = studentApps.OrderByDescending(a => a.DateSubmitted).FirstOrDefault()?.DateSubmitted
        };

        return View(vm);
    }

    [HttpPost]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> UploadProfilePicture(IFormFile photo)

    {

        var email = HttpContext.Session.GetString("StudentEmail");

        if (string.IsNullOrEmpty(email))

            return RedirectToAction("Login", "Student");

        if (photo == null || photo.Length == 0)

        {

            TempData["Error"] = "Please select an image to upload.";

            return RedirectToAction("Profile");

        }

        // allowed file types

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        var ext = Path.GetExtension(photo.FileName)?.ToLowerInvariant();

        if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))

        {

            TempData["Error"] = "Only JPG, PNG, GIF, WEBP formats are allowed.";

            return RedirectToAction("Profile");

        }

        const long maxSize = 2 * 1024 * 1024;

        if (photo.Length > maxSize)

        {

            TempData["Error"] = "Image size must be below 2 MB.";

            return RedirectToAction("Profile");

        }

        try

        {

            var uploadDir = Path.Combine(_env.WebRootPath, "profile_pics");

            if (!Directory.Exists(uploadDir))

                Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";

            var path = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(path, FileMode.Create))

            {

                await photo.CopyToAsync(stream);

            }

            var relativePath = $"/profile_pics/{fileName}";

            // update session + DB

            HttpContext.Session.SetString("StudentPhotoPath", relativePath);

            var student = _db.Students.FirstOrDefault(s => s.Email == email);

            if (student != null)

            {

                // remove old image if exists

                if (!string.IsNullOrEmpty(student.PhotoPath) && !student.PhotoPath.Contains("default.jpg"))

                {

                    var oldFullPath = Path.Combine(_env.WebRootPath, student.PhotoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldFullPath))

                        System.IO.File.Delete(oldFullPath);

                }

                student.PhotoPath = relativePath;

                _db.Students.Update(student);

                await _db.SaveChangesAsync();

            }

            TempData["Success"] = "Profile picture uploaded successfully!";

        }

        catch (Exception ex)

        {

            TempData["Error"] = "Error uploading image: " + ex.Message;

        }

        return RedirectToAction("Profile");

    }


    [HttpPost]

    [ValidateAntiForgeryToken]

    public IActionResult RemoveProfilePicture()

    {

        var email = HttpContext.Session.GetString("StudentEmail");

        if (string.IsNullOrEmpty(email))

            return RedirectToAction("Login", "Student");

        try

        {

            var student = _db.Students.FirstOrDefault(s => s.Email == email);

            if (student == null)

            {

                TempData["Error"] = "Student not found.";

                return RedirectToAction("Profile");

            }

            var defaultPath = "/Images/default.jpg";

            var currentPath = HttpContext.Session.GetString("StudentPhotoPath") ?? student.PhotoPath;

            // delete old file if not default

            if (!string.IsNullOrEmpty(currentPath) && !currentPath.Contains("default.jpg"))

            {

                var fullPath = Path.Combine(_env.WebRootPath, currentPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (System.IO.File.Exists(fullPath))

                    System.IO.File.Delete(fullPath);

            }

            // reset photo to default

            HttpContext.Session.SetString("StudentPhotoPath", defaultPath);

            student.PhotoPath = defaultPath;

            _db.Students.Update(student);

            _db.SaveChanges();

            TempData["Success"] = "Profile picture removed successfully.";

        }

        catch (Exception ex)

        {

            TempData["Error"] = "Error removing profile picture: " + ex.Message;

        }

        return RedirectToAction("Profile");

    }





    // ------------------- Forgot Password Step 1 -------------------
    // ---------- Step 1: Show forgot password form ----------
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: user enters email to start reset
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        email = email?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("", "Please enter your registered email.");
            return View();
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Email == email);
        if (student == null)
        {
            // For security you can show generic message; for dev, show explicit:
            ModelState.AddModelError("", "No account found for that email.");
            return View();
        }

        // save the student id in session for the next reset step (short-lived)
        HttpContext.Session.SetInt32("ResetStudentId", student.Id);

        // redirect to ResetPassword GET which will display the security question
        return RedirectToAction(nameof(ResetPassword));
    }

    // ---------- Step 2: Display security question & reset form ----------
    [HttpGet]
    public IActionResult ResetPassword()
    {
        var sid = HttpContext.Session.GetInt32("ResetStudentId");
        if (sid == null)
        {
            // session expired or user jumped directly here
            TempData["Error"] = "Session expired or invalid access. Start 'Forgot password' again.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var student = _db.Students.Find(sid.Value);
        if (student == null)
        {
            HttpContext.Session.Remove("ResetStudentId");
            TempData["Error"] = "Student not found. Start again.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        ViewBag.SecurityQuestion = student.SecurityQuestion ?? "Security question not set";
        return View();
    }

    // POST: validate answer and set new password
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string securityAnswer, string newPassword, string confirmPassword)
    {
        var sid = HttpContext.Session.GetInt32("ResetStudentId");
        if (sid == null)
        {
            ModelState.AddModelError("", "Session expired. Start the forgot password flow again.");
            return RedirectToAction(nameof(ForgotPassword));
        }

        var student = await _db.Students.FindAsync(sid.Value);
        if (student == null)
        {
            HttpContext.Session.Remove("ResetStudentId");
            ModelState.AddModelError("", "Student not found.");
            return RedirectToAction(nameof(ForgotPassword));
        }

        ViewBag.SecurityQuestion = student.SecurityQuestion;

        // Basic validation
        if (string.IsNullOrWhiteSpace(securityAnswer))
        {
            ModelState.AddModelError("", "Please provide the security answer.");
            return View();
        }
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ModelState.AddModelError("", "Please enter a new password.");
            return View();
        }
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "New password and confirm password do not match.");
            return View();
        }

        // Verify the security answer. Case-insensitive trim compare.
        var savedAnswer = student.SecurityAnswer ?? "";
        if (!string.Equals(savedAnswer.Trim(), securityAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Security answer does not match.");
            return View();
        }

        // Hash and save new password
        student.PasswordHash = _passwordHasher.HashPassword(student, newPassword.Trim());
        try
        {
            _db.Students.Update(student);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error saving new password: " + ex.GetBaseException().Message);
            return View();
        }

        // success — clear session and redirect to login
        HttpContext.Session.Remove("ResetStudentId");
        TempData["Success"] = "Password reset successfully. Please login with your new password.";
        return RedirectToAction("Login"); // or Student/Login depending on your routes
    }


}