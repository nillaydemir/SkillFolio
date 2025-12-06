using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SkillFolio.Models;

namespace SkillFolio.Data
{
    // IdentityDbContext'ten 3 parametre ile türetme KRİTİKTİR.
    public class SkillFolioDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public SkillFolioDbContext(DbContextOptions<SkillFolioDbContext> options)
            : base(options)
        {
        }
        public DbSet<Comment> Comments { get; set; }
        // Domain Modelleri
        public DbSet<Event> Events { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }

        // KRİTİK EKLENTİLER (CS1061 hatasını çözer)
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}