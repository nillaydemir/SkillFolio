using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillFolio.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Duyuru Grubu")]
        public int AnnouncementGroupId { get; set; }

      
        [ForeignKey(nameof(AnnouncementGroupId))]
        public AnnouncementGroup? AnnouncementGroup { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
