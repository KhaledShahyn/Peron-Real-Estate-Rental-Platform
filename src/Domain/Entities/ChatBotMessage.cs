namespace FinalProject.src.Domain.Entities
{
    public class ChatBotMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public string MessageText { get; set; }
        public string Sender { get; set; } 
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
