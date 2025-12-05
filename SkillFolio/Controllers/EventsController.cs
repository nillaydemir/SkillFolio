using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SkillFolio.Controllers
{
    public class EventsController : Controller
    {
        private readonly SkillFolioDbContext _context;

        public EventsController(SkillFolioDbContext context)
        {
            _context = context;
        }

        // 1. READ (List) - Part 1, Part 2, Part 4: Arama ve Sıralama
        [AllowAnonymous] // Herkesin erişimine açık
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentSearch"] = searchString;

            var events = _context.Events.Include(e => e.Category).AsQueryable();

            // Arama Filtresi Uygulama
            if (!String.IsNullOrEmpty(searchString))
            {
                events = events.Where(s => s.Title.Contains(searchString)
                                       || s.Description.Contains(searchString)
                                       || s.Category.Name.Contains(searchString));
            }

            // Sıralama Uygulama
            switch (sortOrder)
            {
                case "title_desc":
                    events = events.OrderByDescending(s => s.Title);
                    break;
                case "Date":
                    events = events.OrderBy(s => s.DatePosted);
                    break;
                case "date_desc":
                    events = events.OrderByDescending(s => s.DatePosted);
                    break;
                default:
                    events = events.OrderBy(s => s.Title);
                    break;
            }

            return View(await events.ToListAsync());
        }

        // 2. READ (Details) - Part 1, Part 2
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            // ... (Kod aynı kalır) ...
            if (id == null) return NotFound();
            var @event = await _context.Events.Include(e => e.Category).FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();
            return View(@event);
        }

        // 3. CREATE (GET/POST) - Part 2, Part 3 (Admin Yetkilendirmesi)
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

        // 4. UPDATE (GET/POST) - Part 2, Part 3 (Admin Yetkilendirmesi)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            // ... (Kod aynı kalır) ...
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
            // ... (Kod aynı kalır) ...
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

        // 5. DELETE (GET/POST) - Part 2, Part 3 (Admin Yetkilendirmesi)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            // ... (Kod aynı kalır) ...
            if (id == null) return NotFound();
            var @event = await _context.Events.Include(e => e.Category).FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();
            return View(@event);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // ... (Kod aynı kalır) ...
            var @event = await _context.Events.FindAsync(id);
            if (@event != null) _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}