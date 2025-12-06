using System;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class Event
    {
        // Birincil Anahtar (Primary Key - Part 2)

        [Key]
        public int EventId { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        // Gerekli Özellik 1: Başlık (Part 2: 3 ek özellik)
        [Required(ErrorMessage = "Etkinlik Başlığı zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Etkinlik Başlığı")]
        public string Title { get; set; }

        // Gerekli Özellik 2: Açıklama (Part 2: 3 ek özellik)
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Detaylı Açıklama")]
        public string Description { get; set; }

        // Gerekli Özellik 3: Kaynak Linki (Part 2: 3 ek özellik)
        // Kullanıcı "Katıl" dediğinde bu linke yönlendirilecek.
        [Required(ErrorMessage = "Harici Kaynak Linki zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [Display(Name = "Kaynak Linki (URL)")]
        public string SourceLink { get; set; }

        // Ek özellik: Etkinliğin yayınlanma tarihi
        [DataType(DataType.DateTime)]
        [Display(Name = "Yayınlanma Tarihi")]
        public DateTime DatePosted { get; set; } = DateTime.Now;

        // Yabancı Anahtar (Foreign Key - Part 2: İlişki)
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        // İlişki Navigasyon Özelliği (Part 2: İlişki)
        public EventCategory Category { get; set; }
    }
} 