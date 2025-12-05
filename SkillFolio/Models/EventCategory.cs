using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class EventCategory
    {
        // Birincil Anahtar (Primary Key - Part 2)
        [Key]
        public int CategoryId { get; set; }

        // Gerekli Özellik 1: Kategori Adı (Part 2: 3 ek özellik)
        [Required(ErrorMessage = "Kategori Adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori Adı 100 karakterden uzun olamaz.")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; }

        // Gerekli Özellik 2: Açıklama (Part 2: 3 ek özellik)
        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        // Gerekli Özellik 3: URL Dostu İsim (Slug) (Part 2: 3 ek özellik)
        // Arama/filtreleme için kullanılabilir.
        public string Slug { get; set; }

        // İlişki: Bir Kategori'nin birden çok Etkinliği olabilir (One-to-Many)
        public ICollection<Event> Events { get; set; }
    }
}