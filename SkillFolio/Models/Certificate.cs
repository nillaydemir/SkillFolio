using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillFolio.Models
{
    public class Certificate
    {
        public int CertificateId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        [Required] 
        [Display(Name = "Bağlı Etkinlik")]
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}