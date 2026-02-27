using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Domain.Entities
{
    public class TokenRequestModel
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
