using System;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public enum AnnouncementTargetType
    {
        All = 0,
        School = 1,
        Department = 2
    }

    public class Announcement
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AnnouncementTargetType TargetType { get; set; } = AnnouncementTargetType.All;

        // TargetType School/Department ise buraya değer girilecek (örn: "İTÜ" / "Bilgisayar Müh.")
        [MaxLength(200)]
        public string? TargetValue { get; set; }
    }
}
