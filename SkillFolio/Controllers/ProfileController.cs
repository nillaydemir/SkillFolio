using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Models;
using SkillFolio.ViewModels;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SkillFolio.Data.SkillFolioDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment; 
    public ProfileController(UserManager<ApplicationUser> userManager, SkillFolio.Data.SkillFolioDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _userManager = userManager;
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // GET: Profile/Index 
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null) return NotFound("Kullanıcı bulunamadı.");

        
        var user = await _context.Users
            .Include(u => u.Certificates)
            .Include(u => u.UserEvent!)
                .ThenInclude(f => f.Event)
            .Include(u => u.Favorites!)
                .ThenInclude(f => f.Event)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound($"Kullanıcı yüklenemedi: ID '{userId}'.");

        // 📅 TAKVİM – KAYITLI+FAV etkinlikler
        var now = DateTime.Now;

        //  Kayıtlı etkinlikler
        var registeredEvents = user.UserEvent?
            .Where(ue => ue.Event != null
                && ue.Event.EventDate.Month == now.Month
                && ue.Event.EventDate.Year == now.Year)
            .Select(ue => ue.Event!)
            ?? Enumerable.Empty<Event>();

        //  Favori etkinlikler
        var favoriteEvents = user.Favorites?
            .Where(f => f.Event != null
                && f.Event.EventDate.Month == now.Month
                && f.Event.EventDate.Year == now.Year)
            .Select(f => f.Event!)
            ?? Enumerable.Empty<Event>();

        //  Birleştir + tekrarları kaldır
        var calendarEvents = registeredEvents
            .Concat(favoriteEvents)
            .DistinctBy(e => e.EventId);

        //  Gün bazlı grupla
        var eventsByDay = calendarEvents
            .GroupBy(e => e.EventDate.Day)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Title).ToList()
            );

        var calendar = new CalendarViewModel
        {
            Year = now.Year,
            Month = now.Month,
            EventsByDay = eventsByDay
        };

        ViewBag.Calendar = calendar;

        return View(user);
    }

    // GET: Profile/Edit 
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Kullanıcı yüklenemedi.");
        }

        
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

      
        if (!ModelState.IsValid)
        {
            model.ExistingProfileImagePath = user.ProfileImagePath;
            return View(model);
        }

        //  Dosya Yükleme İşlemi (Profil Fotoğrafı)
        if (model.ProfilePictureFile != null)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string extension = Path.GetExtension(model.ProfilePictureFile.FileName);
            string fileName = Guid.NewGuid().ToString() + extension;

            user.ProfileImagePath = "/images/profile/" + fileName;
            string path = Path.Combine(wwwRootPath, "images", "profile", fileName);

           
            if (!string.IsNullOrEmpty(model.ExistingProfileImagePath))
            {
                string oldPath = Path.Combine(wwwRootPath, model.ExistingProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            // Yeni fotoğrafı kaydetme
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await model.ProfilePictureFile.CopyToAsync(fileStream);
            }
        }

        // Kullanıcı Bilgilerini Güncelleme
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.SchoolName = model.SchoolName;
        user.Department = model.Department;
        user.BirthDate = model.BirthDate;
        user.StartYear = model.StartYear ?? 0; 
        user.EndYear = model.EndYear;

        // Kullanıcıyı veritabanında güncelleme
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

        
        model.ExistingProfileImagePath = user.ProfileImagePath;
        return View(model);
    }

    // --- Sertifika---

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