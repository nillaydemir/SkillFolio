using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SkillFolio.Controllers
{
    //sadece admin rolündekiler 
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SkillFolioDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, SkillFolioDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Admin Paneli Ana Sayfası
        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Yönetim Paneli";

            return View();
        }

        // Admin Announcements Management
        public async Task<IActionResult> Announcements()
        {
            var announcements = await _context.Announcements
                .Include(a => a.AnnouncementGroup)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        // GET: Admin/CreateAnnouncement
        public async Task<IActionResult> CreateAnnouncement()
        {
            var groups = await _context.AnnouncementGroups
                .Where(g => g.GroupType == "School" || g.GroupType == "Department")
                .ToListAsync();

            if (!groups.Any())
            {
                // If groups don't exist, seed them
                if (!await _context.AnnouncementGroups.AnyAsync(g => g.GroupType == "School"))
                {
                    _context.AnnouncementGroups.Add(new AnnouncementGroup
                    {
                        Name = "Okul Duyuruları",
                        GroupType = "School"
                    });
                }

                if (!await _context.AnnouncementGroups.AnyAsync(g => g.GroupType == "Department"))
                {
                    _context.AnnouncementGroups.Add(new AnnouncementGroup
                    {
                        Name = "Bölüm Duyuruları",
                        GroupType = "Department"
                    });
                }

                await _context.SaveChangesAsync();
                groups = await _context.AnnouncementGroups
                    .Where(g => g.GroupType == "School" || g.GroupType == "Department")
                    .ToListAsync();
            }

            ViewBag.AnnouncementGroupId = new SelectList(groups, "AnnouncementGroupId", "Name");
            return View();
        }

        // POST: Admin/CreateAnnouncement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedAt = System.DateTime.Now;
                _context.Add(announcement);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Duyuru başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Announcements));
            }

            var groups = await _context.AnnouncementGroups
                .Where(g => g.GroupType == "School" || g.GroupType == "Department")
                .ToListAsync();

            if (!groups.Any())
            {
                // If groups don't exist, seed them
                if (!await _context.AnnouncementGroups.AnyAsync(g => g.GroupType == "School"))
                {
                    _context.AnnouncementGroups.Add(new AnnouncementGroup
                    {
                        Name = "Okul Duyuruları",
                        GroupType = "School"
                    });
                }

                if (!await _context.AnnouncementGroups.AnyAsync(g => g.GroupType == "Department"))
                {
                    _context.AnnouncementGroups.Add(new AnnouncementGroup
                    {
                        Name = "Bölüm Duyuruları",
                        GroupType = "Department"
                    });
                }

                await _context.SaveChangesAsync();
                groups = await _context.AnnouncementGroups
                    .Where(g => g.GroupType == "School" || g.GroupType == "Department")
                    .ToListAsync();
            }

            ViewBag.AnnouncementGroupId = new SelectList(groups, "AnnouncementGroupId", "Name", announcement.AnnouncementGroupId);
            return View(announcement);
        }
    }
}