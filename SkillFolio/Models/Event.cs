using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class Event
    {
        
        [Key]
        public int EventId { get; set; }
        public ICollection<Comment>? Comments { get; set; }

      
        [Required(ErrorMessage = "Etkinlik Başlığı zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Etkinlik Başlığı")]
        public string Title { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Detaylı Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Harici Kaynak Linki zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [Display(Name = "Kaynak Linki (URL)")]
        public string SourceLink { get; set; } = string.Empty;

        [Display(Name = "Etkinlik Görseli Yolu")]
        public string? ImagePath { get; set; } 

        [DataType(DataType.DateTime)]
        [Display(Name = "Yayınlanma Tarihi")]
        public DateTime DatePosted { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Etkinlik Türü")]
        public string EventType { get; set; } = "Eğitim";
        // Örn: "Zirve", "Sohbet", "Eğitim"

        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        public EventCategory? Category { get; set; }

        

    }

}