using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; 
namespace SkillFolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly SkillFolioDbContext _context; // Alan tanýmlý

        // KURUCU: Dependency Injection ile DbContext alýnýyor.
        public HomeController(SkillFolioDbContext context)
        {
            _context = context; // Veri atamasý yapýlýyor (Null hatasý çözüldü)
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanýndan veri çekme, _context null olmayacaktýr.
            var featuredEvents = await _context.Events
                .Include(e => e.Category)
                .OrderByDescending(e => e.DatePosted)
                .Take(3)
                .ToListAsync();

            return View(featuredEvents);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}