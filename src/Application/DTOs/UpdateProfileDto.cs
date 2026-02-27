namespace FinalProject.src.Application.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public string? PhoneNumber { get; set; }
    }

}
