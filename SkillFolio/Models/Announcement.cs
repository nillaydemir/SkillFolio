using System;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Duyuru İçeriği")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Duyuru Grubu")]
        public int AnnouncementGroupId { get; set; }

        public AnnouncementGroup AnnouncementGroup { get; set; } = null!;
    }
}
