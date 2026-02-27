using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace FinalProject.src.Presentation.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly UserManager<ApplicatiopnUser> _userManager;

        public RatingController(IRatingService ratingService, UserManager<ApplicatiopnUser> userManager)
        {
            _ratingService = ratingService;
            _userManager = userManager;
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRatings()
        {
            var ratings = await _ratingService.GetAllRatingsAsync();
            return Ok(ratings);
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddRating([FromBody] CreateRatingDto dto)
        {
            try
            {
                var result = await _ratingService.AddRatingAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateRatingDto dto)
        {
            var result = await _ratingService.UpdateRatingAsync(dto);

            if (result.StartsWith("BAD_REQUEST"))
            {
                return BadRequest(new { message = result.Replace("BAD_REQUEST: ", "") });
            }

            return Ok(new { message = result });
        }


        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _ratingService.DeleteRatingAsync(id);

            if (result.StartsWith("BAD_REQUEST"))
            {
                return BadRequest(new { message = result.Replace("BAD_REQUEST: ", "") });
            }

            return Ok(new { message = result });
        }


        [HttpGet("property/{propertyId}")]
        public async Task<IActionResult> GetRatings(int propertyId)
        {
            var ratings = await _ratingService.GetRatingsForPropertyAsync(propertyId);
            return Ok(ratings);
        }

        
        [HttpGet("most-rating")]
        public async Task<IActionResult> TopRatedProperties()
        {
            var topRatedProperties = await _ratingService.GetTopRatedPropertiesAsync();
            return Ok(topRatedProperties);
        }
        [HttpGet("most-area")]
        public async Task<IActionResult> GetMostArea([FromQuery] int top = 5)
        {
            var result = await _ratingService.GetMostAreaAsync(top);
            return Ok(result);
        }

        [HttpGet("highest-price")]
        public async Task<IActionResult> GetHighestPricedProperties()
        {
            var result = await _ratingService.GetHighestPricedPropertiesAsync();
            return Ok(result);
        }

        [HttpGet("lowest-price")]
        public async Task<IActionResult> GetLowestPricedProperties()
        {
            var result = await _ratingService.GetLowestPricedPropertiesAsync();
            return Ok(result);
        }
    }

}
