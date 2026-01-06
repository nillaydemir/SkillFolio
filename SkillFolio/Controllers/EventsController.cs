using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using SkillFolio.ViewModels;

using System.ComponentModel.DataAnnotations;


namespace SkillFolio.Controllers
{
    public class EventsController : Controller
    {
        private readonly SkillFolioDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment; 

    
        public EventsController(SkillFolioDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // List
        [AllowAnonymous]
        public async Task<IActionResult> Index(string sortOrder, string searchString,string eventType) //giriş yapmadan eriş
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

        // (Details)
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
                //Sertifika yüklemeden yorum yapılamaz
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

        // CREATE (GET/POST) - SADECE Admin
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name");
            return View(new EventViewModel()); 
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model) 
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
                    EventDate = model.EventDate,
                    ImagePath = imagePath,
                    DatePosted = DateTime.Now
                };

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        // UPDATE (GET/POST) - SADECE Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            var viewModel = new EventViewModel
            {
                EventId = @event.EventId,
                Title = @event.Title,
                Description = @event.Description,
                SourceLink = @event.SourceLink,
                CategoryId = @event.CategoryId,
                EventType = @event.EventType,
                EventDate = @event.EventDate,
                ExistingImagePath = @event.ImagePath
            };

            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", @event.CategoryId);
            return View(viewModel); 
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel model) 
        {
            if (id != model.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                var @event = await _context.Events.FindAsync(id);
                if (@event == null) return NotFound();

                string newImagePath = @event.ImagePath ?? string.Empty;

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
                @event.EventDate = model.EventDate;
                @event.ImagePath = newImagePath; 

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


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id) // Bu, tarayıcıdan gelen GET isteğini karşılar.
        {
            if (id == null) return NotFound();

            // Onay sayfasında gösterebilmek için ilgili Event ve Category verilerini çekiyoruz
            var @event = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            // Delete.cshtml View'ine Event modelini gönder
            return View(@event);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {

                DeleteImageFile(@event.ImagePath);
                _context.Events.Remove(@event);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string uploadsFolder = Path.Combine(wwwRootPath, "images", "events");

            // Klasör yoksa oluştur
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(file.FileName);
            string fileName = Guid.NewGuid().ToString() + extension;

            string relativePath = $"/images/events/{fileName}";
            string path = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return relativePath;
        }

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