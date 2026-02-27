namespace FinalProject.src.Application.DTOs
{
    public class PendingPropertyDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public string RentType { get; set; }
        public bool IsFurnished { get; set; }
        public int? Area { get; set; }
        public List<string> ImageUrls { get; set; }
    }

}
