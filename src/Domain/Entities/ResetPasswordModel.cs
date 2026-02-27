using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Domain.Entities
{
    public class ResetPasswordModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string OtpCode { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; }
    }
}

