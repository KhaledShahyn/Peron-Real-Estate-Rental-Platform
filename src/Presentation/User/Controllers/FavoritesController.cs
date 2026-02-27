using FinalProject.src.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject.src.Presentation.User.Controllers
{
    [Authorize(Roles = "User,ADMIN")]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly UserManager<ApplicatiopnUser> _userManager;

        public FavoritesController(IFavoriteService favoriteService, UserManager<ApplicatiopnUser> userManager)
        {
            _favoriteService = favoriteService;
            _userManager = userManager;
        }

        [HttpPost("add/{propertyId}")]
        public async Task<IActionResult> AddToFavorites(int propertyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var favorites = await _favoriteService.AddToFavoritesAsync(userId, propertyId);

            if (favorites == null)
                return NotFound(new { message = "الشقة غير موجودة" });

            return Ok(new
            {
                message = "تمت الإضافة إلى المفضلة",
                favorites 
            });
        }


        [HttpDelete("remove/{propertyId}")]
        public async Task<IActionResult> RemoveFromFavorites(int propertyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var favorites = await _favoriteService.RemoveFromFavoritesAsync(userId, propertyId);

            return Ok(new
            {
                message = "تم الحذف من المفضلة",
                favorites 
            });
        }


        [HttpGet("user-favorites")]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }
    }
}
