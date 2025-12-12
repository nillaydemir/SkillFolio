using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillFolio.Models;
using System.Threading.Tasks;
using SkillFolio.Data;
using Microsoft.EntityFrameworkCore;
using System;

// Part 3: Controller'ın tamamını sadece Admin rolündeki kullanıcılara kısıtlar.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    // DbContext veya diğer servisler buraya enjekte edilebilir
    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Admin Paneli Ana Sayfası
    public IActionResult Index()
    {
        ViewData["Title"] = "Admin Yönetim Paneli";
        // Buraya Admin istatistikleri veya görev listesi gelebilir.
        return View();
    }

    // Duyuru Grubu Mesajı Gönderme İşlevi (Projenizin özel ihtiyacı)
    // Bu sadece bir yer tutucudur. Gerçek mesajlaşma mantığı burada uygulanmalıdır.
    public IActionResult PostAnnouncement()
    {
        ViewData["Title"] = "Duyuru Gönder";
        return View(); // Duyuru gönderme formunu gösterir
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> PostAnnouncement(string title, string message, AnnouncementTargetType targetType, string? targetValue)
{
    if (string.IsNullOrWhiteSpace(title))
        ModelState.AddModelError("", "Başlık boş olamaz.");

    if (string.IsNullOrWhiteSpace(message))
        ModelState.AddModelError("", "Duyuru mesajı boş olamaz.");

    if (targetType != AnnouncementTargetType.All && string.IsNullOrWhiteSpace(targetValue))
        ModelState.AddModelError("", "Okul/Bölüm hedefi seçtiysen değer girmen gerekiyor.");

    if (!ModelState.IsValid)
        return View();

    var ann = new Announcement
    {
        Title = title.Trim(),
        Message = message.Trim(),
        TargetType = targetType,
        TargetValue = string.IsNullOrWhiteSpace(targetValue) ? null : targetValue.Trim(),
        CreatedAt = DateTime.UtcNow
    };

    _context.Announcements.Add(ann);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Duyuru başarıyla gönderildi!";
    return RedirectToAction(nameof(PostAnnouncement));
}


    private readonly SkillFolioDbContext _context;

public AdminController(UserManager<ApplicationUser> userManager, SkillFolioDbContext context)
{
    _userManager = userManager;
    _context = context;
}


    // ... Kullanıcıları yönetme, Sertifikaları onaylama gibi diğer Admin metotları buraya eklenebilir ...
}