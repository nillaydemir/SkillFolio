using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // LINQ ve List için eklendi

var builder = WebApplication.CreateBuilder(args);

// --- 1. Hizmetleri Yapýlandýrma ---

// 1.1 Veritabaný Baðlantýsýný Okuma
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 1.2 DbContext'i EF Core'a Kaydetme
builder.Services.AddDbContext<SkillFolioDbContext>(options =>
    options.UseSqlServer(connectionString));

// 1.3 Identity Hizmetlerini Kaydetme
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SkillFolioDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();


// --- 2. Uygulama Ýstek Hattýný (Pipeline) Yapýlandýrma ---

var app = builder.Build();

// Rol ve Kategori Tohumlama metodunu çaðýr
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Metot adý SeedData olarak güncellendi ve bu metot çaðrýlýyor
    await SeedData(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kimlik Doðrulama ve Yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


// --- 3. Rol ve Kategori Tohumlama Metodu (SeedData) ---

static async Task SeedData(IServiceProvider serviceProvider)
{
    var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    // DbContext'i tohumlama için al
    var context = serviceProvider.GetRequiredService<SkillFolioDbContext>();

    // 3.1. Rolleri oluþtur
    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await RoleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await RoleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 3.2. Varsayýlan Admin Kullanýcýsý Oluþturma
    string adminEmail = "admin@skillfolio.com";
    string adminPassword = "Admin123*";

    var adminUser = await UserManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            SchoolName = "SkillFolio Central",
            Department = "IT",
            StartYear = 2020,
            BirthDate = DateTime.Now.AddYears(-20)
        };

        var createAdmin = await UserManager.CreateAsync(newAdminUser, adminPassword);

        if (createAdmin.Succeeded)
        {
            await UserManager.AddToRoleAsync(newAdminUser, "Admin");
        }
    }

    // 3.3. YENÝ EKLENTÝ: Kategori Tohumlamasý (ZORUNLU ALAN DÜZELTMESÝ YAPILDI)
    var newCategories = new List<string>
    {
        "Yazýlým Geliþtirme (Web/Mobil)",
        "Veri Bilimi ve Yapay Zeka",
        "Bulut Teknolojileri (AWS, Azure)",
        "Dijital Pazarlama ve SEO",
        "Grafik Tasarým ve UI/UX",
        "Finans ve Ekonomi",
        "Kariyer ve Kiþisel Geliþim",
        "Giriþimcilik ve Ýnovasyon",
        "Biyoloji ve Yaþam Bilimleri",
        "Sanat ve Yaratýcýlýk"
    };

    foreach (var categoryName in newCategories)
    {
        if (!await context.EventCategories.AnyAsync(c => c.Name == categoryName))
        {
            context.EventCategories.Add(new SkillFolio.Models.EventCategory
            {
                Name = categoryName,
                // KRÝTÝK DÜZELTME: Description alanýna zorunlu bir deðer atandý
                Description = $"Bu, {categoryName} kategorisine ait genel bir açýklamadýr."
            });
        }
    }

    // Deðiþiklikleri veritabanýna kaydet
    await context.SaveChangesAsync();
}