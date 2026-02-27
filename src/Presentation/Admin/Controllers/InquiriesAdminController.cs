using FinalProject.src.Application.DTOs;
using FinalProject.src.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.src.Presentation.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="ADMIN")]
    public class InquiriesAdminController : ControllerBase
    {
        private readonly IInquiryService _inquiryService;
        public InquiriesAdminController(IInquiryService inquiryService)
        {
            _inquiryService = inquiryService;
        }
        [HttpGet("all-inquiries")]
        public async Task<IActionResult> GetInquiries()
        {
            var list = await _inquiryService.GetAllInquiriesAsync();
            return Ok(list);
        }

        [HttpPost("reply-inquiry")]
        public async Task<IActionResult> ReplyInquiry([FromBody] InquiryReplyDto replyDto)
        {
            if (replyDto == null || string.IsNullOrEmpty(replyDto.Reply))
            {
                return BadRequest(new { message = "الرد مطلوب" });
            }

            await _inquiryService.ReplyToInquiryAsync(replyDto.Id, replyDto.Reply);
            return Ok(new { message = "تم الرد بنجاح" });
        }


    }
}
