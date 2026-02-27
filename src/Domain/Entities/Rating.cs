using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FinalProject.src.Domain.Entities
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Range(1, 5)]
        public int? Stars { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; } = null!;
        public ApplicatiopnUser User { get; set; } = null!;

        [ForeignKey("Property")]
        public int PropertyId { get; set; }
        [JsonIgnore]
        public Property Property { get; set; } = null!;
        [NotMapped]
        public string TimeAgo => GetTimeAgo(CreatedAt);

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;

            if (span.TotalMinutes < 1)
                return "الآن";
            if (span.TotalMinutes < 60)
                return $"منذ {(int)span.TotalMinutes} دقيقة";
            if (span.TotalHours < 24)
                return $"منذ {(int)span.TotalHours} ساعة";
            if (span.TotalDays < 30)
                return $"منذ {(int)span.TotalDays} يوم";

            return dateTime.ToString("dd/MM/yyyy");
        } 
    }

}
