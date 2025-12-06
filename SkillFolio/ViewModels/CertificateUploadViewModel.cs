using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.ViewModels
{
    // YENİ ÖZELLİK: Etkinlik ID'si eklendi
    public class CertificateUploadViewModel
    {
        [Required(ErrorMessage = "Sertifika başlığı zorunludur.")]
        [Display(Name = "Sertifika Başlığı/Adı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen sertifika dosyasını seçin.")]
        [Display(Name = "Sertifika Dosyası (PDF, JPG, PNG)")]
        public IFormFile? CertificateFile { get; set; }

        [Required(ErrorMessage = "Lütfen bu sertifikanın ait olduğu etkinliği seçin.")]
        [Display(Name = "Etkinlik Seçimi")]
        public int EventId { get; set; } // Hangi Event'e bağlı olduğunu tutar
    }
}