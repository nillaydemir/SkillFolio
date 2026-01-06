using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;

[Authorize] 
public class CommentsController : Controller
{
    private readonly SkillFolioDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentsController(SkillFolioDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Yorum Silme 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {

        //yorumu bul
        var comment = await _context.Comments
            .Include(c => c.Event) // Yönlendirme için Event'i dahil et
            .FirstOrDefaultAsync(c => c.CommentId == id);

        if (comment == null)
        {
            TempData["CommentError"] = "Silinmek istenen yorum bulunamadı.";
            return RedirectToAction("Index", "Events");
        }

        //kim silmek istiyo
        var userId = _userManager.GetUserId(User);
        bool isAdmin = User.IsInRole("Admin");
        int eventId = comment.EventId;

        // Yetki Kontrolü:
     
        if (comment.ApplicationUserId != userId && !isAdmin)
        {
            TempData["CommentError"] = "Bu yorumu silmeye yetkiniz bulunmamaktadır.";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        // Yorumu Sil
        _context.Comments.Remove(comment);

        // Değişiklikleri Kaydet
        try
        {
            await _context.SaveChangesAsync();
         
            if (!isAdmin)
            {
                
                TempData["SuccessMessage"] = "Yorumunuz başarıyla silindi. Bu etkinliğe tekrar yorum yapabilirsiniz.";
            }
            else
            {
                
                TempData["SuccessMessage"] = "Yorum başarıyla silindi (Admin).";
            }
        }
        catch (Exception ex)
        {
            TempData["CommentError"] = "Yorum silinirken bir hata oluştu: " + ex.Message;
           
        }

        // Etkinlik detay sayfasına geri dön
        return RedirectToAction("Details", "Events", new { id = eventId });
    }
}