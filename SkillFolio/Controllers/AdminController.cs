using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillFolio.Models;
using System.Threading.Tasks;

//sadece admin rolündekiler 
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Admin Paneli Ana Sayfası
    public IActionResult Index()
    {
        ViewData["Title"] = "Admin Yönetim Paneli";

        return View();
    }

    //duyuru grubuna duyuru atmak
    public IActionResult PostAnnouncement()
    {
        ViewData["Title"] = "Duyuru Gönder";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostAnnouncement(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError("", "Duyuru mesajı boş olamaz.");
            return View();
        }
        // --- BURAYA DUYURU GRUBUNA MESAJ ATMA MANTIĞI EKLENECEK ---
        ViewBag.Message = "Duyuru başarıyla gönderildi!";
        return View("Index");
    }

 
}