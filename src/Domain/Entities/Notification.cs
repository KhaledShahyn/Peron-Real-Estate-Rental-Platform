using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.src.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Title { get; set; } 

        [Required]
        public string Message { get; set; } 

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public ApplicatiopnUser User { get; set; }

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
