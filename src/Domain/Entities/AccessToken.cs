namespace FinalProject.src.Domain.Entities
{
    public class AccessToken
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public ApplicatiopnUser? User { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsRevoked { get; set; }
    }
}
