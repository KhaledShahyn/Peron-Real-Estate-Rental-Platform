using System.Text.Json.Serialization;

namespace FinalProject.src.Domain.Entities
{
    public class AuthModel
    {
        public string? Message { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
        public string? Token { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime? ExpiresOn { get; set; }
        [JsonIgnore]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
