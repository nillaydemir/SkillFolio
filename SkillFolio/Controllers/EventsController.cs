using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using SkillFolio.ViewModels; // Yeni eklendi
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment için
using System.IO; // Dosya işlemleri için

namespace SkillFolio.Controllers
{
    public class EventsController : Controller
    {
        private readonly SkillFolioDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment; // YENİ EKLENDİ

        // KURUCU: DbContext, UserManager ve IWebHostEnvironment enjekte edildi.
        public EventsController(SkillFolioDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // 1. READ (List)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string sortOrder, string searchString,string eventType)
        {
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentEventType"] = eventType;

            var events = _context.Events.Include(e => e.Category).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                events = events.Where(s => s.Title.Contains(searchString)
                                         || s.Description.Contains(searchString)
                                         || s.Category!.Name.Contains(searchString));
            }
            if (!string.IsNullOrEmpty(eventType))
            {
                events = events.Where(e => e.EventType == eventType);
            }

            switch (sortOrder)
            {
                case "title_desc": events = events.OrderByDescending(s => s.Title); break;
                case "Date": events = events.OrderBy(s => s.DatePosted); break;
                case "date_desc": events = events.OrderByDescending(s => s.DatePosted); break;
                default: events = events.OrderBy(s => s.Title); break;
            }

            return View(await events.ToListAsync());
        }

        // 2. READ (Details)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Comments!)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

                bool hasCertificate = await _context.Certificates.AnyAsync(c => c.ApplicationUserId == userId && c.EventId == id);
                bool hasCommented = await _context.Comments.AnyAsync(c => c.ApplicationUserId == userId && c.EventId == id);

                ViewBag.HasCertificate = hasCertificate;
                ViewBag.HasCommented = hasCommented;
                ViewBag.CanComment = hasCertificate && !hasCommented;
            }
            else
            {
                ViewBag.HasCertificate = false;
                ViewBag.HasCommented = false;
                ViewBag.CanComment = false;
            }

            return View(@event);
        }

        // 3. CREATE (GET/POST) - SADECE Admin
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name");
            return View(new EventViewModel()); // ViewModel gönderiliyor
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model) // ViewModel kullanıldı
        {
            if (ModelState.IsValid)
            {
                string imagePath = string.Empty;

                // Fotoğraf Yükleme İşlemi
                if (model.ImageFile != null)
                {
                    imagePath = await SaveImageFile(model.ImageFile);
                }

                var @event = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    SourceLink = model.SourceLink,
                    CategoryId = model.CategoryId,
                    EventType = model.EventType,
                    ImagePath = imagePath, // Yolu kaydet
                    DatePosted = DateTime.Now
                };

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        // 4. UPDATE (GET/POST) - SADECE Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            // Event modelini ViewModel'e dönüştür
            var viewModel = new EventViewModel
            {
                EventId = @event.EventId,
                Title = @event.Title,
                Description = @event.Description,
                SourceLink = @event.SourceLink,
                CategoryId = @event.CategoryId,
                EventType = @event.EventType,
                ExistingImagePath = @event.ImagePath
            };

            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", @event.CategoryId);
            return View(viewModel); // ViewModel gönderildi
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel model) // ViewModel kullanıldı
        {
            if (id != model.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                var @event = await _context.Events.FindAsync(id);
                if (@event == null) return NotFound();

                string newImagePath = @event.ImagePath ?? string.Empty;

                // 1. Yeni Fotoğraf Yükleme ve Eski Fotoğrafı Silme
                if (model.ImageFile != null)
                {
                    // Eski fotoğrafı sil
                    DeleteImageFile(@event.ImagePath);

                    // Yeni fotoğrafı kaydet
                    newImagePath = await SaveImageFile(model.ImageFile);
                }

                // 2. Modeli Güncelleme
                @event.Title = model.Title;
                @event.Description = model.Description;
                @event.SourceLink = model.SourceLink;
                @event.CategoryId = model.CategoryId;
                @event.EventType = model.EventType;
                @event.ImagePath = newImagePath; // Yolu güncelle

                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        // 5. DELETE (POST) - SADECE Admin
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                // İlişkili fotoğrafı sunucudan sil
                DeleteImageFile(@event.ImagePath);

                // Veritabanından sil
                _context.Events.Remove(@event);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // ------------------------------------
        // YARDIMCI METOTLAR (Helper Methods)
        // ------------------------------------

        // Dosyayı wwwroot/images/events klasörüne kaydeder
        private async Task<string> SaveImageFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string uploadsFolder = Path.Combine(wwwRootPath, "images", "events");

            // Klasör yoksa oluştur
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(file.FileName);
            string fileName = Guid.NewGuid().ToString() + extension;

            // DB'ye kaydedilecek göreceli yol
            string relativePath = $"/images/events/{fileName}";
            string path = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return relativePath;
        }

        // Eski fotoğrafı sunucudan siler
        private void DeleteImageFile(string? imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string oldPath = Path.Combine(wwwRootPath, imagePath.TrimStart('/'));

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }
        }

        // ... (Yorum ve Favorileme Metotları aynı kalır) ...

        // Yorum Gönderme Modeli
        public class CommentViewModel
        {
            [Required] public int EventId { get; set; }
            [Required]
            [StringLength(500, MinimumLength = 5, ErrorMessage = "Yorum 5 ile 500 karakter arasında olmalıdır.")]
            [Display(Name = "Yorumunuz")]
            public string Content { get; set; } = string.Empty;
        }

        // Yorumu kaydetme
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CommentViewModel model)
        {
            // ... (AddComment mantığı aynı kalır) ...
            var userId = _userManager.GetUserId(User);

            bool hasCertificate = await _context.Certificates.AnyAsync(c => c.ApplicationUserId == userId && c.EventId == model.EventId);
            if (!hasCertificate)
            {
                TempData["CommentError"] = "Yorum yapabilmek için önce bu etkinliğe ait sertifikanızı yüklemelisiniz.";
                return RedirectToAction("Details", new { id = model.EventId });
            }

            bool hasExistingComment = await _context.Comments.AnyAsync(c => c.ApplicationUserId == userId && c.EventId == model.EventId);
            if (hasExistingComment)
            {
                TempData["CommentError"] = "Bu etkinliğe zaten yorum yaptınız. Tekrar yorum yapamazsınız.";
                return RedirectToAction("Details", new { id = model.EventId });
            }

            if (ModelState.IsValid)
            {
                var comment = new Comment
                {
                    EventId = model.EventId,
                    ApplicationUserId = userId,
                    Content = model.Content,
                    DatePosted = DateTime.Now
                };
                _context.Comments.Add(comment);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Yorumunuz başarıyla kaydedildi!";
                }
                catch (Exception)
                {
                    TempData["CommentError"] = "Yorum kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.";
                }
            }

            return RedirectToAction("Details", new { id = model.EventId });
        }


        // FAVORİ EKLEME/ÇIKARMA
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            // ... (ToggleFavorite mantığı aynı kalır) ...
            var userId = _userManager.GetUserId(User);

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.EventId == eventId && f.ApplicationUserId == userId);

            if (existingFavorite != null)
            {
                _context.Favorites.Remove(existingFavorite);
            }
            else
            {
                var newFavorite = new Favorite
                {
                    EventId = eventId,
                    ApplicationUserId = userId,
                    DateFavorited = DateTime.Now
                };
                _context.Favorites.Add(newFavorite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = eventId });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}