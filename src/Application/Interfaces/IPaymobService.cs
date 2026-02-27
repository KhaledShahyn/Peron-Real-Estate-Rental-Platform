using FinalProject.src.Domain.Entities;

namespace FinalProject.src.Application.Interfaces
{
    public interface IPaymobService
    {
        Task<string> CreatePaymentRequestAsync(PendingProperty pending);
    }
}
