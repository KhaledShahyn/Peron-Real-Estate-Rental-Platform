using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Domain.Entities
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        [ForeignKey("Receiver")]
        public string ReceiverId { get; set; }

        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        public ApplicatiopnUser Sender { get; set; }
        public ApplicatiopnUser Receiver { get; set; }
    }
}
