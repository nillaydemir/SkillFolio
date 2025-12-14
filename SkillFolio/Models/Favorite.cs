using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillFolio.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? User { get; set; }

        public DateTime DateFavorited { get; set; } = DateTime.Now;
    }
}