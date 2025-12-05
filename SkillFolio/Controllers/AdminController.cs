using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillFolio.Models;
using System.Threading.Tasks;

// Part 3: Controller'ın tamamını sadece Admin rolündeki kullanıcılara kısıtlar.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    // DbContext veya diğer servisler buraya enjekte edilebilir
    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Admin Paneli Ana Sayfası
    public IActionResult Index()
    {
        ViewData["Title"] = "Admin Yönetim Paneli";
        // Buraya Admin istatistikleri veya görev listesi gelebilir.
        return View();
    }

    // Duyuru Grubu Mesajı Gönderme İşlevi (Projenizin özel ihtiyacı)
    // Bu sadece bir yer tutucudur. Gerçek mesajlaşma mantığı burada uygulanmalıdır.
    public IActionResult PostAnnouncement()
    {
        ViewData["Title"] = "Duyuru Gönder";
        return View(); // Duyuru gönderme formunu gösterir
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
        // Örn: Veritabanına kaydetme, tüm kullanıcılara e-posta gönderme vb.

        ViewBag.Message = "Duyuru başarıyla gönderildi!";
        return View("Index");
    }

    // ... Kullanıcıları yönetme, Sertifikaları onaylama gibi diğer Admin metotları buraya eklenebilir ...
}