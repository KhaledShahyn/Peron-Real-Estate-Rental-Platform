using FinalProject.src.Domain.Entities;

namespace FinalProject.src.Application.Interfaces
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(PendingProperty property);
    }

}
