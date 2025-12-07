using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize] // Bu Controller'daki tüm metotlar için giriş zorunludur
public class CommentsController : Controller
{
    private readonly SkillFolioDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentsController(SkillFolioDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Yorum Silme Metodu (Hem Kullanıcı hem de Admin için)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _context.Comments
            .Include(c => c.Event) // Yönlendirme için Event'i dahil et
            .FirstOrDefaultAsync(c => c.CommentId == id);

        if (comment == null)
        {
            TempData["CommentError"] = "Silinmek istenen yorum bulunamadı.";
            return RedirectToAction("Index", "Events");
        }

        var userId = _userManager.GetUserId(User);
        bool isAdmin = User.IsInRole("Admin");
        int eventId = comment.EventId;

        // Yetki Kontrolü:
        // 1. Kullanıcı kendi yorumunu mu siliyor? VEYA
        // 2. Kullanıcı Admin mi?
        if (comment.ApplicationUserId != userId && !isAdmin)
        {
            TempData["CommentError"] = "Bu yorumu silmeye yetkiniz bulunmamaktadır.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        // 1. Yorumu Sil
        _context.Comments.Remove(comment);

        // 2. Değişiklikleri Kaydet
        try
        {
            await _context.SaveChangesAsync();

            // Başarı Mesajı ve Mantık Yönetimi
            if (!isAdmin)
            {
                // Kullanıcı kendi yorumunu sildiyse, yorum yapma hakkı geri gelir.
                TempData["SuccessMessage"] = "Yorumunuz başarıyla silindi. Bu etkinliğe tekrar yorum yapabilirsiniz.";
            }
            else
            {
                // Admin sildiyse
                TempData["SuccessMessage"] = "Yorum başarıyla silindi (Admin).";
            }
        }
        catch (Exception ex)
        {
            TempData["CommentError"] = "Yorum silinirken bir hata oluştu: " + ex.Message;
            // Gerçek projede loglama yapılmalı
        }

        // Etkinlik detay sayfasına geri dön
        return RedirectToAction("Details", "Events", new { id = eventId });
    }
}