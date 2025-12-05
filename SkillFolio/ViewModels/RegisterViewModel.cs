using System;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.ViewModels
{
    public class RegisterViewModel
    {
        // 7. E-mail
        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        // 1. Name
        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        // 2. Surname
        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        // 10. Birth Date
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime BirthDate { get; set; }

        // 3. School
        [Required]
        [Display(Name = "Okul Adı")]
        public string SchoolName { get; set; }

        // 4. Department
        [Required]
        [Display(Name = "Bölüm")]
        public string Department { get; set; }

        // 5. Start year
        [Required]
        [Range(1950, 2050, ErrorMessage = "Geçerli bir başlangıç yılı girin.")]
        [Display(Name = "Başlangıç Yılı")]
        public int StartYear { get; set; }

        // 6. End year
        [Range(1950, 2050, ErrorMessage = "Geçerli bir bitiş yılı girin.")]
        [Display(Name = "Bitiş Yılı")]
        public int? EndYear { get; set; } // Mock-up'ta zorunlu olmasına rağmen, mantıksal olarak nullable bıraktık.

        // 8. Password
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        // 9. Password again
        [DataType(DataType.Password)]
        [Display(Name = "Şifreyi Onayla")]
        [Compare("Password", ErrorMessage = "Şifre ve onay şifresi eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}