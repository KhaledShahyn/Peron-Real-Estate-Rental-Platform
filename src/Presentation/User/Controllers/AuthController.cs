using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject.src.Presentation.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthController(IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _authService.RegisterAsync(model);
            if (result.Errors != null && result.Errors.Any())
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("Send-Otp")]
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailRequestDto email)
        {
            var result = await _authService.SendVerificationCodeAsync(email);

            if (result == null)
                return BadRequest(new { Message = "فشل إرسال كود التحقق أو المستخدم غير موجود!" });

            return Ok(new { Message = result });
        }
        [HttpPost("Verify-Otp-ForConfirmedEmail")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel model)
        {
            var result = await _authService.VerifyOtpAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _authService.LoginAsync(model);
            if (!result.IsAuthenticated)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailRequestDto otp)
        {
            var result = await _authService.ForgotPasswordAsync(otp);

            if (result == null)
                return BadRequest(new { Message = "الايميل غير مسجل أو حدث خطأ أثناء إرسال كود التحقق!" });

            return Ok(new { result.Message });
        }


        [HttpPost("Check-Otp-For-ResetPassword")]
        public async Task<IActionResult> VerifyOtpForRestPass([FromBody] VerifyOtpModel model)
        {
            var result = await _authService.VerifyOtpForResetPassAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var result = await _authService.ResetPasswordAsync(model);

            if (result == null)
                return BadRequest(new { Message = "حدث خطأ غير متوقع!" });

            if (!string.IsNullOrEmpty(result.Message) && !result.IsAuthenticated)
            {
                return NotFound(new { result.Message });
            }

            return Ok(new { result.Message });
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { Message = "المستخدم غير مصادق عليه!" });

            var result = await _authService.LogoutAsync(userId);
            if (result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }


        [Authorize(Roles = "ADMIN")]
        [HttpPost("add-role")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleModel model)
        {
            var result = await _authService.AddRoleAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(new { Message = result });

            return Ok(new { Message = "Role added successfully!" });
        }
        //[Authorize(Roles = "User")]
        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> RefreshToken()
        //{ 
        //    var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

        //    if (string.IsNullOrEmpty(refreshToken))
        //        return BadRequest(new { message = "Refresh token is missing from cookies" });

        //    var result = await _authService.RefreshTokenAsync();
        //    if (!result.IsAuthenticated)
        //        return BadRequest(result);

        //    return Ok(result);
        //}
        //[Authorize(Roles = "User")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _authService.RefreshTokenAsync();

            if (!result.IsAuthenticated)
                return BadRequest(result);

            return Ok(result);
        }


        [Authorize(Roles = "ADMIN")]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] string token)
        {
            var result = await _authService.RevokeTokenAsync(token);
            if (!result)
                return BadRequest(new { Message = "Invalid token" });

            return Ok(new { Message = "Token revoked successfully" });
        }
    }
}



