using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Application.DTOs
{
    public class EmailRequestDto
    {
        [Required]
        [EmailAddress]
        [JsonProperty("email")]
        public string? Email { get; set; }
    }

}
