using FinalProject.src.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ApplicatiopnUser : IdentityUser
{
    
    public string FullName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public double? Rating { get; set; }
    public DateTime? AccountCreationDate { get; set; }
    public List<RefreshToken>? RefreshTokens { get; set; }
    public string? Status { get; set; }
    public string? OtpCode { get; set; }

    public DateTime? CodeExpiryTime { get; set; }

    [JsonIgnore]
    public List<Property>? Properties { get; set; }

    [JsonIgnore]
    public List<Booking>? Bookings { get; set; }

    [JsonIgnore]
    public List<Message>? SentMessages { get; set; }
    public int FailedOtpAttempts { get; set; } = 0;
    public DateTime? OtpLockoutEnd { get; set; }

    [JsonIgnore]
    public List<Message>? ReceivedMessages { get; set; }
    public List<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<AccessToken> AccessTokens { get; set; } = new List<AccessToken>();
    public ICollection<Rating>? Ratings { get; set; }

}
