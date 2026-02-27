namespace FinalProject.src.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class ChatMessage
    {
        public int Id { get; set; }

        [MaxLength(450)] // هذا مهم جداً
        public string SenderId { get; set; }

        [MaxLength(450)] // هذا أيضاً
        public string ReceiverId { get; set; }

        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }

        public virtual ApplicatiopnUser Sender { get; set; }
        public virtual ApplicatiopnUser Receiver { get; set; }
    }

}
