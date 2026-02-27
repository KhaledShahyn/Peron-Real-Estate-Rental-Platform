using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using FinalProject.src.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.src.Presentation.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InquiryController : ControllerBase
    {
        private readonly IInquiryService _inquiryService;
        private readonly IAuthService _authService;
        public InquiryController(IInquiryService inquiryService,IAuthService authService)
        {
                _inquiryService = inquiryService;
                _authService = authService;
        }
        [HttpPost("submit-inquiry")]
        public async Task<IActionResult> SubmitInquiry([FromBody] InquiryDto dto)
        {
            await _inquiryService.SubmitInquiryAsync(dto);
            return Ok(new { message = "تم إرسال استفسارك بنجاح" });
        }
        [HttpGet("my-inquiries")]
        public async Task<IActionResult> GetMyInquiries()
        {
            var inquiries = await _inquiryService.GetUserInquiriesAsync();
            return Ok(inquiries);
        }

    }
}
