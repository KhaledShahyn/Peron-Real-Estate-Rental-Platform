namespace FinalProject.src.Application.DTOs
{
    public class RatingResponseDto
    {
        public int RatingId { get; set; }
        public int PropertyId { get; set; }
        public int? Stars { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
