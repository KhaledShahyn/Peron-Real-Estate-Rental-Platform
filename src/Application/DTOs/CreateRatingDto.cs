namespace FinalProject.src.Application.DTOs
{
    public class CreateRatingDto
    {
        public int PropertyId { get; set; }
        public int? Stars { get; set; }
        public string? Comment { get; set; }
    }
}
