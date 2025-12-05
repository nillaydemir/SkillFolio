using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillFolio.Data;
using SkillFolio.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Hizmetleri Yapýlandýrma ---

// 1.1 Veritabaný Baðlantýsýný Okuma
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 1.2 DbContext'i EF Core'a Kaydetme
builder.Services.AddDbContext<SkillFolioDbContext>(options =>
    options.UseSqlServer(connectionString));

// 1.3 Identity Hizmetlerini Kaydetme (Part 3)
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

// Rol Tohumlama metodunu çaðýr (Admin Rolü ve Varsayýlan Admin Kullanýcýsý Oluþturur)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRoles(services);
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

// Part 3: Kimlik Doðrulama ve Yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


// --- 3. Rol Tohumlama Metodu ---

 static async Task SeedRoles(IServiceProvider serviceProvider)
{
    var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Rolleri oluþtur
    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await RoleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await RoleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Varsayýlan Admin Kullanýcýsý Oluþturma
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
}