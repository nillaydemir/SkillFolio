// ViewModels/ProfileEditViewModel.cs

using Microsoft.AspNetCore.Http; // IFormFile için
using System;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.ViewModels
{
    public class ProfileEditViewModel
    {
        // Temel Kullanıcı Bilgileri
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Okul Adı")]
        public string? SchoolName { get; set; }

        [Display(Name = "Bölüm")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime BirthDate { get; set; } = DateTime.Now;

        [Display(Name = "Başlangıç Yılı")]
        public int? StartYear { get; set; }

        [Display(Name = "Bitiş Yılı (Mezuniyet)")]
        public int? EndYear { get; set; }

        // Profil Fotoğrafı Yükleme Alanı
        [Display(Name = "Yeni Profil Fotoğrafı Seç")]
        public IFormFile? ProfilePictureFile { get; set; }

        // Mevcut fotoğrafın yolu (Gösterim ve silme mantığı için)
        public string? ExistingProfileImagePath { get; set; }
    }
}