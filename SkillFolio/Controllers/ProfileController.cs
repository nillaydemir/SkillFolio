using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için eklendi
using Microsoft.EntityFrameworkCore;
using SkillFolio.Models;
using SkillFolio.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly SkillFolio.Data.SkillFolioDbContext _context;

    public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment, SkillFolio.Data.SkillFolioDbContext context)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
        _context = context;
    }

    // ... Index metodu aynı kalır ...
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null) return NotFound("Kullanıcı bulunamadı.");

        var user = await _context.Users
            .Include(u => u.Certificates)
            .Include(u => u.Favorites!).ThenInclude(f => f.Event)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound($"Kullanıcı ID'sine sahip kullanıcı yüklenemedi: {userId}.");

        var favoritedEvents = user.Favorites?.Where(f => f.Event != null).Select(f => new { Title = f.Event!.Title + " (Favori)", Date = f.DateFavorited.Date }) ?? Enumerable.Empty<dynamic>();
        var certificatedEvents = user.Certificates?.Select(c => new { Title = c.Title + " (Sertifika)", Date = c.UploadDate.Date }) ?? Enumerable.Empty<dynamic>();

        ViewBag.CalendarEvents = favoritedEvents.Union(certificatedEvents).ToList();

        return View(user);
    }

    // --- UploadCertificate (GET) ---
    public IActionResult UploadCertificate()
    {
        // YENİ EKLENTİ: Tüm etkinlikleri View'e gönderiyoruz
        ViewBag.EventId = new SelectList(_context.Events.OrderBy(e => e.Title), "EventId", "Title");
        return View(new CertificateUploadViewModel());
    }

    // --- UploadCertificate (POST) ---
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCertificate(CertificateUploadViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Dosya Yükleme İşlemi (Önceki kodunuzdaki gibi)
            string uniqueFileName = string.Empty;
            if (model.CertificateFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "certificates");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CertificateFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CertificateFile!.CopyToAsync(fileStream);
                }
            }

            var certificate = new Certificate
            {
                Title = model.Title,
                FilePath = uniqueFileName,
                ApplicationUserId = user.Id,
                UploadDate = DateTime.Now,
                // KRİTİK EKLENTİ: Seçilen EventId'yi Certificate modeline atama
                EventId = model.EventId
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Eğer model geçerli değilse, listeyi tekrar yükle
        ViewBag.EventId = new SelectList(_context.Events.OrderBy(e => e.Title), "EventId", "Title", model.EventId);
        return View(model);
    }
}