using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinalProject.src.Domain.Entities
{
    public class PropertyImage
    {
        [Key]
        public int ImageId { get; set; }

        [ForeignKey("Property")]
        public int PropertyId { get; set; }
        [JsonIgnore]
        public Property Property { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public string Caption { get; set; } = "Default Caption";
    }
}
