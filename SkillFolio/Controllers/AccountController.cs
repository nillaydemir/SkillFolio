using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; // CS0103 ('View', 'ModelState') hataları için kritik
using SkillFolio.Models;
using SkillFolio.ViewModels;
using System.Threading.Tasks;

// Sınıfın kapsamını belirleyen ad alanı
namespace SkillFolio.Controllers
{
    // Controller tanımı (CS0106 ve CS8802 hatalarını çözer)
    public class AccountController : Controller
    {
        // Bağımlılık Enjeksiyonu için Alanlar (CS0103 '_userManager' hatasını çözer)
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Kurucu (Constructor)
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --- REGISTER (GET) ---

        public IActionResult Register()
        {
            return View();
        }

        // --- REGISTER (POST) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    // Tüm Mockup Alanlarının Atanması
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    SchoolName = model.SchoolName,
                    Department = model.Department,
                    StartYear = model.StartYear,
                    EndYear = model.EndYear, // Nullable ise bu şekilde atanabilir
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Başarılı kayıttan sonra kullanıcıyı otomatik giriş yap
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Hata oluşursa Model State'e ekle
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // --- LOGIN (GET) ---

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // --- LOGIN (POST) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Giriş yapma denemesi
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Giriş başarılıysa hedef sayfaya yönlendir
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }
            return View(model);
        }

        // --- LOGOUT (POST) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}