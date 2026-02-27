using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Helper;
using Stripe.Checkout;
using Stripe;
using FinalProject.src.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FinalProject.src.Infrastructure.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeSettings _settings;

        public StripeService(IOptions<StripeSettings> options)
        {
            _settings = options.Value;
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(PendingProperty property)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            UnitAmount = 4000, // 40.00 EGP
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Add Listing Fee: {property.Title}",
                                Description = "Service fee for adding your property"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"https://localhost:4200/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"https://sakaniapi1.runasp.net/payment/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "pendingPropertyId", property.PropertyId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }
    }
}
