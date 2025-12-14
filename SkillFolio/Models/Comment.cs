
using System.ComponentModel.DataAnnotations;

namespace SkillFolio.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Yorum içeriği boş bırakılamaz.")]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;

        public DateTime DatePosted { get; set; } = DateTime.Now;

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}