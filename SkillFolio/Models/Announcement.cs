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
        public string Title { get; set; }

        [Required]
        [Display(Name = "Duyuru İçeriği")]
        public string Content { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //RELATIONSHIP
        [Required]
        [Display(Name = "Duyuru Grubu")]
        public int AnnouncementGroupId { get; set; }

        public AnnouncementGroup AnnouncementGroup { get; set; }
    }
}
