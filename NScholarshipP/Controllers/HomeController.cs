using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NScholarshipP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NScholarshipP.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // simple admin credentials (for demo purposes only)
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        // In-memory list of announcements
        private static List<Announcement> announcements = new List<Announcement>();
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;


        public IActionResult Contact()
        {
            return View(); // Views/Home/Contact.cshtml (or integrate into Index)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                // return view with validation messages
                return View(model);
            }

            try
            {
                _db.ContactMessages.Add(model);
                await _db.SaveChangesAsync();

                TempData["ContactSuccess"] = "Thanks — your message has been received. We'll get back to you soon.";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                // log error if you have logger
                ModelState.AddModelError(string.Empty, "An error occurred while sending your message. Please try again.");
                return View(model);
            }
        }

        public IActionResult Index()
        {
            var recent = _db.Announcements
                            .Where(a => a.IsPublic)
                            .OrderByDescending(a => a.DatePosted)
                            .ToList();
            return View(recent); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Helpdesk()
        {
            return View();
        }

        // -------------------- Admin Login --------------------
        public ActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminLogin(string username, string password)
        {
            if (username == AdminUsername && password == AdminPassword)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("AnnouncementsAdmin");
            }

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        // Helper to ensure admin access
        private bool IsAdmin() => HttpContext.Session.GetString("IsAdmin") == "true";
        // -------------------- Announcementspublic list --------------------
        public IActionResult AnnouncementsPublic()
        {
            var list = _db.Announcements
                          .OrderByDescending(a => a.DatePosted)
                          .ToList();

            return View("AnnouncementsPublic", list);
        }

        public IActionResult AnnouncementsAdmin()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("AdminLogin");

            var list = _db.Announcements.OrderByDescending(a => a.DatePosted).ToList();
            return View("AnnouncementsAdmin", list);
        }

        // -------------------- Create --------------------
        public ActionResult CreateAnnouncement()
        {
            if (!IsAdmin())
                return RedirectToAction("AdminLogin");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAnnouncement(Announcement announcement)
        {
            if (!ModelState.IsValid) return View(announcement);
            if (_db == null) throw new InvalidOperationException("Database context not available.");

            announcement.DatePosted = DateTime.UtcNow;
            _db.Announcements.Add(announcement);
            var saved = _db.SaveChanges();

            if (!IsAdmin())
                return RedirectToAction("AdminLogin");

            if (announcement == null)
            {
                TempData["ErrorMessage"] = "Invalid announcement data.";
                return RedirectToAction("AnnouncementsAdmin");
            }
            announcement.Id = announcements.Any() ? announcements.Max(a => a.Id) + 1 : 1;
            announcement.DatePosted = DateTime.Now;
            announcements.Add(announcement);

            TempData["SuccessMessage"] = "Announcement added successfully!";
            return RedirectToAction("AnnouncementsAdmin");
        }

        // -------------------- Edit --------------------
        public IActionResult EditAnnouncement(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("AdminLogin");

            var ann = _db.Announcements.Find(id);
            if (ann == null)
            {
                TempData["ErrorMessage"] = "Announcement not found.";
                return RedirectToAction("AnnouncementsAdmin");
            }
            return View(ann);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAnnouncement(Announcement updated)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("AdminLogin");

            if (!ModelState.IsValid) return View(updated);

            var ann = _db.Announcements.Find(updated.Id);
            if (ann == null)
            {
                TempData["ErrorMessage"] = "Announcement not found.";
                return RedirectToAction("AnnouncementsAdmin");
            }

            ann.Title = updated.Title;
            ann.Message = updated.Message;
            ann.IsPublic = updated.IsPublic;

            _db.SaveChanges();
            TempData["SuccessMessage"] = "Announcement updated.";
            return RedirectToAction("AnnouncementsAdmin");
        }

        // -------------------- Details --------------------
        public IActionResult DetailsAnnouncement(int id)
        {
            var announcement = _db.Announcements.FirstOrDefault(a => a.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }


        // -------------------- Delete --------------------
        // GET: Delete
        public IActionResult DeleteAnnouncement(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("AdminLogin");

            var ann = _db.Announcements.Find(id);
            if (ann == null)
            {
                TempData["ErrorMessage"] = "Announcement not found.";
                return RedirectToAction("AnnouncementsAdmin");
            }

            return View(ann);
        }

        // POST: Delete 
        [HttpPost, ActionName("DeleteAnnouncement")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAnnouncementConfirmed(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("AdminLogin");

            var ann = _db.Announcements.Find(id);
            if (ann == null)
            {
                TempData["ErrorMessage"] = "Announcement not found.";
                return RedirectToAction("AnnouncementsAdmin");
            }

            _db.Announcements.Remove(ann);
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Announcement deleted.";
            return RedirectToAction("AnnouncementsAdmin");
        }

        // ? ADMIN LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("AdminLogin");
        }

        // -------------------- Error --------------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
