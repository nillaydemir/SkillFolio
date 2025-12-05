using System.ComponentModel.DataAnnotations;

namespace SkillFolio.ViewModels
{
    public class LoginViewModel
    {
        // Kullanıcı Adı / E-posta
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        // Şifre
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        // Beni Hatırla Seçeneği
        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}