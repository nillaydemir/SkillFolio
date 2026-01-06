using System.ComponentModel.DataAnnotations;

namespace SkillFolio.ViewModels
{
    public class RegisterViewModel
    {
      
        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

    
        [Required]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

   
        [Required]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

    
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime BirthDate { get; set; }

        [Required]
        [Display(Name = "Okul Adı")]
        public string SchoolName { get; set; }

        [Required]
        [Display(Name = "Bölüm")]
        public string Department { get; set; }

        [Required]
        [Range(1950, 2050, ErrorMessage = "Geçerli bir başlangıç yılı girin.")]
        [Display(Name = "Başlangıç Yılı")]
        public int StartYear { get; set; }

        [Range(1950, 2050, ErrorMessage = "Geçerli bir bitiş yılı girin.")]
        [Display(Name = "Bitiş Yılı")]
        public int? EndYear { get; set; } // Mock-up'ta zorunlu olmasına rağmen, mantıksal olarak nullable bıraktık.

        //Compare- ASP.NET otomatik olarak Passwprd ve ComfirmPassword karşılaştırır
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifreyi Onayla")]
        [Compare("Password", ErrorMessage = "Şifre ve onay şifresi eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}