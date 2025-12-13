using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // KRİTİK: IdentityDbContext için
using Microsoft.AspNetCore.Identity; // IdentityRole için
using SkillFolio.Models;

namespace SkillFolio.Data
{
    // DÜZELTME: IdentityDbContext'ten 3 genel (generic) parametre ile türetildi.
    // 1. ApplicationUser (Kullanıcı modeli)
    // 2. IdentityRole (Rol modeli)
    // 3. string (Anahtar tipi - Varsayılan)
    public class SkillFolioDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public SkillFolioDbContext(DbContextOptions<SkillFolioDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }

        public DbSet<EventCategory> EventCategories { get; set; }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementGroup> AnnouncementGroups { get; set; }

       // public DbSet<Certificate> Certificates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Bu çağrı KRİTİKTİR. Identity'nin tüm tablolarının (Roller dahil) kurulmasını sağlar.
            base.OnModelCreating(builder);
        }
    }
}