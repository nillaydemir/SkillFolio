using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Okul Adı zorunludur.")]
        public string SchoolName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bölüm zorunludur.")]
        public string Department { get; set; } = string.Empty;

        // 5. Start year (Başlangıç Yılı)
        [Required(ErrorMessage = "Başlangıç Yılı zorunludur.")]
        [Display(Name = "Başlangıç Yılı")]
        public int StartYear { get; set; } // int olarak tutulması daha uygun

        // 6. End year (Bitiş Yılı)
        // Opsiyonel olabilir (mezun olmadıysa), ancak mock-up'ta zorunlu görünüyor
        [Display(Name = "Bitiş Yılı")]
        public int? EndYear { get; set; } // Nullable int olarak tutabiliriz

        // 10. Birth Date (Doğum Tarihi)
        [Required(ErrorMessage = "Doğum Tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime BirthDate { get; set; }
    }
}