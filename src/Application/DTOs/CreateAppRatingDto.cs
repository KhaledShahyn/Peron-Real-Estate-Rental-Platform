using System.ComponentModel.DataAnnotations;

namespace FinalProject.src.Application.DTOs
{
    public class CreateAppRatingDto
    {
        [Range(1,5)]
        public int Stars {  get; set; }
    }
}
