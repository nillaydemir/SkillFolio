using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SkillFolio.Models
{
    public class Event
    {
        // Birincil Anahtar
        [Key]
        public int EventId { get; set; }

        // Navigasyon Özelliği (Yorumlar)
        public ICollection<Comment>? Comments { get; set; }

        // Gerekli Özellik 1: Başlık
        [Required(ErrorMessage = "Etkinlik Başlığı zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Etkinlik Başlığı")]
        public string Title { get; set; } = string.Empty; // CS8618 hatası önlendi

        // Gerekli Özellik 2: Açıklama
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Detaylı Açıklama")]
        public string Description { get; set; } = string.Empty;

        // Gerekli Özellik 3: Kaynak Linki
        [Required(ErrorMessage = "Harici Kaynak Linki zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [Display(Name = "Kaynak Linki (URL)")]
        public string SourceLink { get; set; } = string.Empty;

        // YENİ EKLENTİ: Etkinlik Fotoğrafının Yolu (DB'de tutulacak)
        [Display(Name = "Etkinlik Görseli Yolu")]
        public string? ImagePath { get; set; } // Nullable olmalı (mevcut etkinlikler için)

        // Ek özellik: Etkinliğin yayınlanma tarihi
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

        // Yabancı Anahtar (Kategori)
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        // İlişki Navigasyon Özelliği
        public EventCategory? Category { get; set; }

        

    }

}