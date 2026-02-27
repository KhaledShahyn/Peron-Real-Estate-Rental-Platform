namespace FinalProject.src.Application.DTOs
{
    public class UpdateRatingDto
    {
        public int RatingId { get; set; }
        public int? Stars { get; set; }
        public string? Comment { get; set; }
    }
}
