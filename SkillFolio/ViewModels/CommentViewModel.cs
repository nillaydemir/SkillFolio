using System.ComponentModel.DataAnnotations;

namespace SkillFolio.ViewModels
{
    public class CommentViewModel
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 5,
            ErrorMessage = "Yorum 5 ile 500 karakter arasında olmalıdır.")]
        public string Content { get; set; } = string.Empty;
    }
}
