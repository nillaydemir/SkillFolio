using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;

[Authorize(Roles = "Admin")]
public class AdminAnnouncementsController : Controller
{
    private readonly SkillFolioDbContext _context;

    public AdminAnnouncementsController(SkillFolioDbContext context)
    {
        _context = context;
    }

    // ===================== LIST =====================
    public async Task<IActionResult> Index()
    {
        var announcements = await _context.Announcements
            .Include(a => a.AnnouncementGroup)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return View(announcements);
    }

    // ===================== CREATE (GET) =====================
    public IActionResult Create()
    {
        // ❌ Event Reminder (AnnouncementGroupId = 3) admin tarafından seçilemez
        ViewBag.Groups = _context.AnnouncementGroups //school, department +/  remainder - 
            .Where(g => g.AnnouncementGroupId != 3)
            .ToList();

        return View();
    }

    // ===================== CREATE (POST) =====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Announcement model)
    {
        // Güvenlik: manuel POST ile bile engelle
        if (model.AnnouncementGroupId == 3)
        {
            ModelState.AddModelError(
                "",
                "Bu duyuru grubu sistem tarafından otomatik yönetilir."
            );
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Groups = _context.AnnouncementGroups
                .Where(g => g.AnnouncementGroupId != 3)
                .ToList();

            return View(model);
        }

        model.CreatedAt = DateTime.Now;

        _context.Announcements.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
