namespace FinalProject.src.Domain.Entities
{
    public class Inquiry
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public string? AdminReply { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RepliedAt { get; set; }
    }

}
