using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Models;
using SkillFolio.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SkillFolio.Data.SkillFolioDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment; // Fotoğraf yükleme için eklendi

    public ProfileController(UserManager<ApplicationUser> userManager, SkillFolio.Data.SkillFolioDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _userManager = userManager;
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // GET: Profile/Index - Profil sayfasını ve verileri gösterir
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null) return NotFound("Kullanıcı bulunamadı.");

        // Eager Loading: Sertifikalar, Favoriler ve ilişkili Etkinlikleri yükler
        var user = await _context.Users
            .Include(u => u.Certificates)
            .Include(u => u.Favorites!)
                .ThenInclude(f => f.Event)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound($"Kullanıcı yüklenemedi: ID '{userId}'.");

        // 📅 TAKVİM – SADECE KULLANICIYA AİT EVENTLER
    var now = DateTime.Now;

    var userEvents =
        user.Favorites?
            .Where(f => f.Event != null)
            .Select(f => f.Event!)
            .Union(
                user.Certificates?
                    .Where(c => c.Event != null)
                    .Select(c => c.Event!)
                ?? Enumerable.Empty<Event>()
            )
            .Distinct()
            .ToList()
        ?? new List<Event>();

    var calendar = new CalendarViewModel
    {
        Year = now.Year,
        Month = now.Month,
        EventsByDay = userEvents
            .Where(e =>
                e.EventDate.Month == now.Month &&
                e.EventDate.Year == now.Year)
            .GroupBy(e => e.EventDate.Day)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Title).ToList()
            )
    };

    ViewBag.Calendar = calendar;

    return View(user);
}

    // GET: Profile/Edit - Profili düzenleme formunu gösterir
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Kullanıcı yüklenemedi.");
        }

        // Kullanıcı verilerini ViewModel'e eşle
        var viewModel = new ProfileEditViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            SchoolName = user.SchoolName,
            Department = user.Department,
            BirthDate = user.BirthDate,
            StartYear = user.StartYear,
            EndYear = user.EndYear,
            ExistingProfileImagePath = user.ProfileImagePath
        };

        return View(viewModel);
    }

    // POST: Profile/Edit - Profili günceller ve fotoğrafı kaydeder
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // ModelState geçerli olmasa bile mevcut fotoğraf yolu korunmalı
        if (!ModelState.IsValid)
        {
            model.ExistingProfileImagePath = user.ProfileImagePath;
            return View(model);
        }

        // 1. Dosya Yükleme İşlemi (Profil Fotoğrafı)
        if (model.ProfilePictureFile != null)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string extension = Path.GetExtension(model.ProfilePictureFile.FileName);
            string fileName = Guid.NewGuid().ToString() + extension;

            // DB'ye kaydedilecek göreceli yol (wwwroot'a göre)
            user.ProfileImagePath = "/images/profile/" + fileName;
            string path = Path.Combine(wwwRootPath, "images", "profile", fileName);

            // Eski fotoğrafı silme (opsiyonel ama iyi bir pratik)
            if (!string.IsNullOrEmpty(model.ExistingProfileImagePath))
            {
                string oldPath = Path.Combine(wwwRootPath, model.ExistingProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            // Yeni fotoğrafı kaydetme
            // Klasörün varlığını kontrol et (Gerekirse oluştur)
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await model.ProfilePictureFile.CopyToAsync(fileStream);
            }
        }

        // 2. Kullanıcı Bilgilerini Güncelleme
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.SchoolName = model.SchoolName;
        user.Department = model.Department;
        user.BirthDate = model.BirthDate;
        user.StartYear = model.StartYear ?? 0; // Nullable olmayan alanlar için varsayılan değer atanmalı
        user.EndYear = model.EndYear;

        // Kullanıcıyı veritabanında güncelle
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Identity hatalarını gösterme
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        // Hata durumunda mevcut fotoğraf yolunu tekrar View'e gönder
        model.ExistingProfileImagePath = user.ProfileImagePath;
        return View(model);
    }

    // --- Sertifika Yükleme Metotları ---

    // GET: Profile/UploadCertificate
    public IActionResult UploadCertificate()
    {
        ViewBag.EventId = new SelectList(_context.Events.OrderBy(e => e.Title), "EventId", "Title");
        return View(new CertificateUploadViewModel());
    }

    // POST: Profile/UploadCertificate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCertificate(CertificateUploadViewModel model)
    {
        // ... (Sertifika yükleme mantığı aynı kalır) ...
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            string uniqueFileName = string.Empty;
            if (model.CertificateFile != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "certificates");
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
                EventId = model.EventId
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewBag.EventId = new SelectList(_context.Events.OrderBy(e => e.Title), "EventId", "Title", model.EventId);
        return View(model);
    }
}