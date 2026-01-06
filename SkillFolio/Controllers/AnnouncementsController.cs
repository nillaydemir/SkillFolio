using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using SkillFolio.ViewModels;


namespace SkillFolio.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly SkillFolioDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnnouncementsController(
         SkillFolioDbContext context,
         UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            // Admin ayrı kalsın 
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");



            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return View(new List<Event>()); // ASLA NULL DÖNME

            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);

            var events = await _context.Favorites
                .Where(f => f.ApplicationUserId == user.Id)
                .Include(f => f.Event)
                .Select(f => f.Event)
                .Where(e =>
                    e != null &&
                    e.EventDate >= today &&
                    e.EventDate <= nextWeek)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return View(events); 
        }

     

        // Announcements groups
        public async Task<IActionResult> Groups()
        {
            // Admin buraya girmesin
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var viewModel = new AnnouncementGroupsViewModel();

            // ===== OKUL DUYURULARI =====
            if (!string.IsNullOrEmpty(user.SchoolName))
            {
                var schoolAnnouncements = await _context.Announcements
                    .Include(a => a.AnnouncementGroup)
                    .Where(a =>
                        a.AnnouncementGroup.GroupType == "School" &&
                        a.AnnouncementGroup.Name == user.SchoolName)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                if (schoolAnnouncements.Any())
                {
                    viewModel.HasSchoolGroup = true;
                    viewModel.SchoolAnnouncements = schoolAnnouncements;
                }
            }

            // ===== BÖLÜM DUYURULARI =====
            if (!string.IsNullOrEmpty(user.Department))
            {
                var departmentAnnouncements = await _context.Announcements
                    .Include(a => a.AnnouncementGroup)
                    .Where(a =>
                        a.AnnouncementGroup.GroupType == "Department" &&
                        a.AnnouncementGroup.Name == user.Department)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                if (departmentAnnouncements.Any())
                {
                    viewModel.HasDepartmentGroup = true;
                    viewModel.DepartmentAnnouncements = departmentAnnouncements;
                }
            }
            return View(viewModel);


        }
    }
}
