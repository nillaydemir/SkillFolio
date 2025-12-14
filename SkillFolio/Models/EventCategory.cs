
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class EventCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Kategori Adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori Adı 100 karakterden uzun olamaz.")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; }
        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }
        public ICollection<Event> Events { get; set; }
    }
}