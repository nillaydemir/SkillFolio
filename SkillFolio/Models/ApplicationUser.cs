using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace SkillFolio.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Zorunlu alanlar 
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

        public ICollection<Certificate>? Certificates { get; set; } 
        public ICollection<Favorite>? Favorites { get; set; }
        public ICollection<UserEvent> UserEvent { get; set; }

    }
}