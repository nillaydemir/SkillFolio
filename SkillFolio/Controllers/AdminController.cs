using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SkillFolio.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SkillFolioDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            SkillFolioDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ANNOUNCEMENTS
        public async Task<IActionResult> Announcements()
        {
            var announcements = await _context.Announcements
                .Include(a => a.AnnouncementGroup)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        //  CREATE GROUP 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(string name, string groupType)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(groupType))
            {
                TempData["GroupError"] = "Grup adı ve türü zorunludur.";
                return RedirectToAction(nameof(CreateAnnouncement));
            }

            bool exists = await _context.AnnouncementGroups
                .AnyAsync(g => g.Name == name && g.GroupType == groupType);

            if (exists)
            {
                TempData["GroupError"] = "Bu grup zaten mevcut.";
                return RedirectToAction(nameof(CreateAnnouncement));
            }

            _context.AnnouncementGroups.Add(new AnnouncementGroup
            {
                Name = name,
                GroupType = groupType
            });

            await _context.SaveChangesAsync();

            TempData["GroupSuccess"] = "Duyuru grubu oluşturuldu.";
            return RedirectToAction(nameof(CreateAnnouncement));
        }

        //  CREATE ANNOUNCEMENT (GET) 
        public async Task<IActionResult> CreateAnnouncement()
        {
            var groups = await EnsureBaseGroupsAsync();

            ViewBag.AnnouncementGroupId = new SelectList(
                groups,
                "AnnouncementGroupId",
                "Name"
            );

            return View();
        }

        //  CREATE ANNOUNCEMENT (POST) 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(Announcement announcement)
        {
            if (!ModelState.IsValid)
            {
                var groups = await EnsureBaseGroupsAsync();
                ViewBag.AnnouncementGroupId = new SelectList(
                    groups,
                    "AnnouncementGroupId",
                    "Name",
                    announcement.AnnouncementGroupId
                );

                return View(announcement);
            }

            announcement.CreatedAt = DateTime.Now;

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Duyuru başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Announcements));
        }

        // ilk başta db de hata almamak için)
        private async Task<IQueryable<AnnouncementGroup>> EnsureBaseGroupsAsync()
        {
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

            return _context.AnnouncementGroups;
        }
    }
}
