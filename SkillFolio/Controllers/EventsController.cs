using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // UserManager için gerekli
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillFolio.Controllers
{
    public class EventsController : Controller
    {
        private readonly SkillFolioDbContext _context;
        // KRİTİK EKLENTİ: Favorileme için UserManager tanımlandı
        private readonly UserManager<ApplicationUser> _userManager;

        // KURUCU: Hem DbContext hem de UserManager enjekte edildi.
        public EventsController(SkillFolioDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. READ (List) - Part 4: Arama ve Sıralama
        [AllowAnonymous]
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentSearch"] = searchString;

            var events = _context.Events.Include(e => e.Category).AsQueryable();

            // Arama ve Sıralama Mantığı...
            if (!String.IsNullOrEmpty(searchString))
            {
                events = events.Where(s => s.Title.Contains(searchString)
                                        || s.Description.Contains(searchString)
                                        || s.Category.Name.Contains(searchString));
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

                // 1. Sertifika Kontrolü (Katılım)
                bool hasCertificate = await _context.Certificates
                    .AnyAsync(c => c.ApplicationUserId == userId && c.EventId == id);

                // 2. Yorum Kontrolü (Tek Yorum Kısıtlaması)
                bool hasCommented = await _context.Comments
                    .AnyAsync(c => c.ApplicationUserId == userId && c.EventId == id);

                // View'e gönderilecek bayraklar
                ViewBag.HasCertificate = hasCertificate;
                ViewBag.HasCommented = hasCommented;

                // Yorum yapma yetkisi: Sertifika var VE daha önce yorum yapmamış
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
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,SourceLink,CategoryId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                @event.DatePosted = System.DateTime.Now;
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", @event.CategoryId);
            return View(@event);
        }

        // 4. UPDATE (GET/POST) - SADECE Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", @event.CategoryId);
            return View(@event);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,SourceLink,DatePosted,CategoryId")] Event @event)
        {
            if (id != @event.EventId) return NotFound();
            if (ModelState.IsValid)
            {
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
            ViewBag.CategoryId = new SelectList(_context.EventCategories, "CategoryId", "Name", @event.CategoryId);
            return View(@event);
        }

        // 5. DELETE (GET/POST) - SADECE Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events.Include(e => e.Category).FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();
            return View(@event);
        }
        public class CommentViewModel
        {
            [Required]
            public int EventId { get; set; }

            [Required]
            [StringLength(500, MinimumLength = 5, ErrorMessage = "Yorum 5 ile 500 karakter arasında olmalıdır.")]
            [Display(Name = "Yorumunuz")]
            public string Content { get; set; } = string.Empty;
        }

        // Yorumu kaydetme (Sadece giriş yapmış kullanıcılar yorum yapabilir)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CommentViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            // Sertifika kontrolü
            bool hasCertificate = await _context.Certificates
                .AnyAsync(c => c.ApplicationUserId == userId && c.EventId == model.EventId);

            if (!hasCertificate)
            {
                TempData["CommentError"] = "Yorum yapabilmek için önce bu etkinliğe ait sertifikanızı yüklemelisiniz.";
                return RedirectToAction("Details", new { id = model.EventId });
            }

            // KRİTİK EKLENTİ: TEK YORUM KONTROLÜ
            bool hasExistingComment = await _context.Comments
                .AnyAsync(c => c.ApplicationUserId == userId && c.EventId == model.EventId);

            if (hasExistingComment)
            {
                TempData["CommentError"] = "Bu etkinliğe zaten yorum yaptınız. Tekrar yorum yapamazsınız.";
                return RedirectToAction("Details", new { id = model.EventId });
            }

            // ... (Kalan yorum kaydetme mantığı aynı kalır) ...
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
                catch (Exception ex)
                {
                    TempData["CommentError"] = "Yorum kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.";
                    // Gerçek projede: ex detaylarını logla
                }
            }

            return RedirectToAction("Details", new { id = model.EventId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null) _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // FAVORİ EKLEME/ÇIKARMA (Part 4)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            var userId = _userManager.GetUserId(User); // _userManager kullanılır

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