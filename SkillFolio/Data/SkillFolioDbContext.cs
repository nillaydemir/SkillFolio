using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SkillFolio.Models;

namespace SkillFolio.Data
{
    public class SkillFolioDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public SkillFolioDbContext(DbContextOptions<SkillFolioDbContext> options)
            : base(options)
        {
        }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }

        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}