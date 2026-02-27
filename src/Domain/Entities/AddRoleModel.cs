using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Domain.Entities
{
    public class AddRoleModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
