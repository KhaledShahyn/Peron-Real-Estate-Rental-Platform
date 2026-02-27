namespace FinalProject.src.Application.DTOs
{
    public class UserAppRatingDto
    {

        public string Name { get; set; }
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int? Rating { get; set; }
        public DateTime? JoinedAt { get; set; }  // تاريخ الانضمام
        public string Location { get; set; }     // الموقع
        public string Channel { get; set; }
    }

}
