using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.src.Domain.Entities
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public ApplicatiopnUser User { get; set; } = null!;
        [ForeignKey("Property")]
        public int PropertyId { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }


        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
