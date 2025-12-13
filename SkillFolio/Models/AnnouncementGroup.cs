using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class AnnouncementGroup
    {
        [Key]
        public int AnnouncementGroupId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Grup Adı")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Grup Türü")]
        public string GroupType { get; set; }
        // Örnek: "School", "Department"

        // Navigation
        public ICollection<Announcement> Announcements { get; set; }
    }
}
