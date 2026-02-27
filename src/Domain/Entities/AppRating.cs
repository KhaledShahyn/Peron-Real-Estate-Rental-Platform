using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.src.Domain.Entities
{
    public class AppRating
    {
        public int Id { get; set; }
        [Range(1,5)]
        public int Stars { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.Now;
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public ApplicatiopnUser? User { get; set; }  
    }
}
