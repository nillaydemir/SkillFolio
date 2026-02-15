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

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        SkillFolio.Data.SkillFolioDbContext context,
        IWebHostEnvironment hostEnvironment)
    {
        _userManager = userManager;
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    //PROFILE INDEX 
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return NotFound("Kullanıcı bulunamadı.");

        var user = await _context.Users
            .Include(u => u.Certificates)
            .Include(u => u.UserEvent!)
                .ThenInclude(ue => ue.Event)
            .Include(u => u.Favorites!)
                .ThenInclude(f => f.Event)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound("Kullanıcı yüklenemedi.");

        var now = DateTime.Now;

        //  CALENDAR 
        var registeredEvents = user.UserEvent?
            .Where(ue => ue.Event != null &&
                         ue.Event.EventDate.Month == now.Month &&
                         ue.Event.EventDate.Year == now.Year)
            .Select(ue => ue.Event!)
            ?? Enumerable.Empty<Event>();

        var favoriteEvents = user.Favorites?
            .Where(f => f.Event != null &&
                        f.Event.EventDate.Month == now.Month &&
                        f.Event.EventDate.Year == now.Year)
            .Select(f => f.Event!)
            ?? Enumerable.Empty<Event>();

        var calendarEvents = registeredEvents
            .Concat(favoriteEvents)
            .DistinctBy(e => e.EventId);

        var eventsByDay = calendarEvents
            .GroupBy(e => e.EventDate.Day)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Title).ToList()
            );

        ViewBag.Calendar = new CalendarViewModel
        {
            Year = now.Year,
            Month = now.Month,
            EventsByDay = eventsByDay
        };

        //  RECOMMENDED EVENTS 
       

        //  Katıldığı etkinliklerden kategori analizi
        var joinedCategoryId = user.UserEvent?
            .Where(ue => ue.Event != null && ue.Event.EventDate < DateTime.Now)
            .GroupBy(ue => ue.Event!.CategoryId)
            .OrderByDescending(g => g.Count())
            .Select(g => (int?)g.Key)
            .FirstOrDefault();

        //  Favori kategoriler
        var favoriteCategoryId = user.Favorites?
            .Where(f => f.Event != null)
            .GroupBy(f => f.Event!.CategoryId)
            .OrderByDescending(g => g.Count())
            .Select(g => (int?)g.Key)
            .FirstOrDefault();

        List<Event> recommendedEvents = new();

        //  Katıldığı kategori
        if (joinedCategoryId.HasValue)
        {
            recommendedEvents = await _context.Events
                .Where(e =>
                    e.CategoryId == joinedCategoryId.Value &&
                    e.EventDate > DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();
        }

        //  Eğer boş geldiyse → Favori kategori
        if (!recommendedEvents.Any() && favoriteCategoryId.HasValue)
        {
            recommendedEvents = await _context.Events
                .Where(e =>
                    e.CategoryId == favoriteCategoryId.Value &&
                    e.EventDate > DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();
        }

        //  Hâlâ boşsa → Genel yaklaşanlar
        if (!recommendedEvents.Any())
        {
            recommendedEvents = await _context.Events
                .Where(e => e.EventDate > DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();
        }

        ViewBag.RecommendedEvents = recommendedEvents;



        return View(user);
    }

    //  EDIT PROFILE 
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        return View(new ProfileEditViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            SchoolName = user.SchoolName,
            Department = user.Department,
            BirthDate = user.BirthDate,
            StartYear = user.StartYear,
            EndYear = user.EndYear,
            ExistingProfileImagePath = user.ProfileImagePath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.ExistingProfileImagePath = user.ProfileImagePath;
            return View(model);
        }

        // Profile image upload
        if (model.ProfilePictureFile != null)
        {
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/profile");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(model.ProfilePictureFile.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                var oldPath = Path.Combine(_hostEnvironment.WebRootPath, user.ProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ProfilePictureFile.CopyToAsync(stream);

            user.ProfileImagePath = "/images/profile/" + fileName;
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.SchoolName = model.SchoolName;
        user.Department = model.Department;
        user.BirthDate = model.BirthDate;
        user.StartYear = model.StartYear ?? 0;
        user.EndYear = model.EndYear;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        model.ExistingProfileImagePath = user.ProfileImagePath;
        return View(model);
    }

    //  CERTIFICATE 
    public IActionResult UploadCertificate()
    {
        ViewBag.EventId = new SelectList(
            _context.Events.OrderBy(e => e.Title),
            "EventId",
            "Title");

        return View(new CertificateUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCertificate(CertificateUploadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventId = new SelectList(
                _context.Events.OrderBy(e => e.Title),
                "EventId",
                "Title",
                model.EventId);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        string fileName = "";

        if (model.CertificateFile != null)
        {
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "certificates");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            fileName = Guid.NewGuid() + "_" + model.CertificateFile.FileName;
            string filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await model.CertificateFile.CopyToAsync(stream);
        }

        _context.Certificates.Add(new Certificate
        {
            Title = model.Title,
            FilePath = fileName,
            ApplicationUserId = user.Id,
            UploadDate = DateTime.Now,
            EventId = model.EventId
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
