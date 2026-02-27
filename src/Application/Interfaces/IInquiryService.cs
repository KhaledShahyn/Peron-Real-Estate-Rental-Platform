using FinalProject.src.Application.DTOs;
using FinalProject.src.Domain.Entities;

namespace FinalProject.src.Application.Interfaces
{
    public interface IInquiryService
    {
        Task SubmitInquiryAsync(InquiryDto dto);
        Task<List<Inquiry>> GetAllInquiriesAsync();
        Task ReplyToInquiryAsync(int inquiryId, string replyMessage);
        Task<List<Inquiry>> GetUserInquiriesAsync();
    }
}
