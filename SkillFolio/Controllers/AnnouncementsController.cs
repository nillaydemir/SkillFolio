using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using SkillFolio.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SkillFolio.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly SkillFolioDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnnouncementsController(SkillFolioDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // INDEX - Shows only favorited events with EventDate between today and today+7 days
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);

            var favoritedEvents = await _context.Favorites
                .Where(f => f.ApplicationUserId == userId)
                .Include(f => f.Event)
                .Select(f => f.Event)
                .Where(e => e != null && e.EventDate >= today && e.EventDate <= nextWeek)
                .OrderBy(e => e!.EventDate)
                .ToListAsync();

            return View(favoritedEvents);
        }

        // GROUPS - Shows School and Department announcement groups
        public async Task<IActionResult> Groups()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var viewModel = new AnnouncementGroupsViewModel
            {
                HasSchoolGroup = !string.IsNullOrEmpty(user.SchoolName),
                HasDepartmentGroup = !string.IsNullOrEmpty(user.Department)
            };

            // School announcements
            if (viewModel.HasSchoolGroup)
            {
                var schoolGroup = await _context.AnnouncementGroups
                    .FirstOrDefaultAsync(g => g.GroupType == "School");
                
                if (schoolGroup != null)
                {
                    viewModel.SchoolAnnouncements = await _context.Announcements
                        .Where(a => a.AnnouncementGroupId == schoolGroup.AnnouncementGroupId)
                        .Include(a => a.AnnouncementGroup)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync();
                }
            }

            // Department announcements
            if (viewModel.HasDepartmentGroup)
            {
                var departmentGroup = await _context.AnnouncementGroups
                    .FirstOrDefaultAsync(g => g.GroupType == "Department");
                
                if (departmentGroup != null)
                {
                    viewModel.DepartmentAnnouncements = await _context.Announcements
                        .Where(a => a.AnnouncementGroupId == departmentGroup.AnnouncementGroupId)
                        .Include(a => a.AnnouncementGroup)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync();
                }
            }

            return View(viewModel);
        }

        // DETAILS - Read-only view for announcements
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements
                .Include(a => a.AnnouncementGroup)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);

            if (announcement == null) return NotFound();

            return View(announcement);
        }
    }
}

