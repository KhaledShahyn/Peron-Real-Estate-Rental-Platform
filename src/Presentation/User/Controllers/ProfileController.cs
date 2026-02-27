using FinalProject.src.Domain.Entities;
using FinalProject.src.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinalProject.src.Application.DTOs;

namespace FinalProject.src.Presentation.User.Controllers
{
    [Authorize(Roles = "User,ADMIN")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly UserProfileService _userProfileService;

        public ProfileController(UserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var profileDto = await _userProfileService.GetProfileAsync();
                return Ok(profileDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //[HttpPost("create")]
        //public async Task<IActionResult> CreateProfile([FromForm] UpdateProfileDto profileDto)
        //{
        //    try
        //    {
        //        var isCreated = await _userProfileService.CreateProfileAsync(profileDto);
        //        if (!isCreated) return BadRequest(new { message = "Failed to create profile" });

        //        return Ok(new { message = "Profile created successfully" });
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto profileDto)
        {
            try
            {
                var isUpdated = await _userProfileService.UpdateProfileAsync(profileDto);
                if (!isUpdated) return BadRequest(new { message = "Failed to update profile" });

                return Ok(new { message = "Profile updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                await _userProfileService.ChangePasswordAsync(model);
                return Ok(new { message = "تم تغيير كلمة المرور بنجاح." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("Get-Property")]
        public async Task<IActionResult> GetProperty()
        {
            try
            {
                var get = await _userProfileService.GetPropertyAsync();
                return Ok(get);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                await _userProfileService.DeleteAccount();
                return Ok(new { message = "تم حذف الحساب بنجاح." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



    }


}
