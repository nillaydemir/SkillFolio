// ViewModels/EventViewModel.cs (YENİ DOSYA OLUŞTURULACAK)

using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; } // Düzenleme için

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

        [Display(Name = "Kategori")]
        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Etkinlik türü zorunludur.")]
        [Display(Name = "Etkinlik Türü")]
        public string EventType { get; set; } = "Eğitim";

        [Required(ErrorMessage = "Etkinlik tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Etkinlik Tarihi")]
        public DateTime EventDate { get; set; }

        // YENİ: Dosya Yükleme Alanı
        [Display(Name = "Etkinlik Görseli Seç")]
        public IFormFile? ImageFile { get; set; }

        // Mevcut fotoğrafın yolu (Düzenleme sayfasında göstermek için)
        public string? ExistingImagePath { get; set; }

    }
}