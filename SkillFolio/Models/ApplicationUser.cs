using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // ICollection için gerekli!

namespace SkillFolio.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Zorunlu alanlar (CS8618 hatalarını önlemek için başlatıldı)
        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Okul")]
        public string SchoolName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bölüm")]
        public string Department { get; set; } = string.Empty;
        [Display(Name = "Profil Fotoğrafı Yolu")]
        public string? ProfileImagePath { get; set; }
        [Required]
        [Display(Name = "Başlangıç Yılı")]
        public int StartYear { get; set; }

        [Display(Name = "Bitiş Yılı")]
        public int? EndYear { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime BirthDate { get; set; }

        // Navigasyon Özellikleri
        public ICollection<Certificate>? Certificates { get; set; } // CS1061 hatasının kaynağı
        public ICollection<Favorite>? Favorites { get; set; }
    }
}