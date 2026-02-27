using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.src.Presentation.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppRatingController : ControllerBase
    {
        private readonly IAppRatingService _ratingService;
        private readonly IAuthService _authService;


        public AppRatingController(IAppRatingService ratingService, IAuthService authService)
        {
            _ratingService = ratingService;
            _authService = authService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddRating([FromBody] CreateAppRatingDto dto)
        {
            var user = await _authService.ValidateUserAsync();
            var result = await _ratingService.AddRatingAsync(dto, user.Id);

            if (!result) return BadRequest("فشل في التقييم");

            return Ok("تم إرسال تقييمك للتطبيق بنجاح");
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetAverage()
        {
            var avg = await _ratingService.GetAverageRatingAsync();
            return Ok(new { averageRating = avg });
        }
    }
}
