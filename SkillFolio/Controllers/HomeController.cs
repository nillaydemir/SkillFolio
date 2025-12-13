using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; 
namespace SkillFolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly SkillFolioDbContext _context; // Alan tan�ml�

        // KURUCU: Dependency Injection ile DbContext al�n�yor.
        public HomeController(SkillFolioDbContext context)
        {
            _context = context; // Veri atamas� yap�l�yor (Null hatas� ��z�ld�)
        }

        public async Task<IActionResult> Index()
        {
            // Veritaban�ndan veri �ekme, _context null olmayacakt�r.
            var featuredEvents = await _context.Events
                .Include(e => e.Category)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync();

                 // 📅 TAKVİM DATASI
            var now = DateTime.Now;

            var calendar = new CalendarViewModel
            {
                Year = now.Year,
                Month = now.Month,
                EventsByDay = await _context.Events
                    .Where(e => 
                    e.DatePosted.Month == now.Month && 
                    e.EventDate.Year == now.Year)
                    .GroupBy(e => e.EventDate.Day)
                    .Select(g => new
                    {
                        Day = g.Key,
                        Titles = g.Select(e => e.Title).ToList()
                    })
                    .ToDictionaryAsync(x => x.Day, x => x.Titles)
            };

            // View’a iki farklı data gönderiyoruz
            ViewBag.Calendar = calendar;

            return View(featuredEvents);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}