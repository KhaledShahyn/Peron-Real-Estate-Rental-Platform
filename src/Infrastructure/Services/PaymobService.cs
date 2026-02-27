using FinalProject.src.Application.Interfaces;
using FinalProject.src.Domain.Entities;
using FinalProject.src.Domain.Helper;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace FinalProject.src.Infrastructure.Services
{
    public class PaymobService:IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public PaymobService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> CreatePaymentRequestAsync(PendingProperty pending)
        {
            // 1. Step: Get Auth Token
            var authResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", new
            {
                api_key = _config["Paymob:ApiKey"]
            });
            var authResult = await authResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
            var token = authResult.token;

            // 2. Step: Register Order
            var orderResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", new
            {
                auth_token = token,
                delivery_needed = false,
                amount_cents = (int)(pending.Price * 100),
                currency = "EGP",
                items = new object[] { },
                merchant_order_id = pending.PropertyId // ده مهم علشان نجيبه بعدين
            });
            var orderResult = await orderResponse.Content.ReadFromJsonAsync<OrderRegistrationResponse>();

            // 3. Step: Generate Payment Key
            var paymentKeyResponse = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", new
            {
                auth_token = token,
                amount_cents = (int)(pending.Price * 100),
                expiration = 3600,
                order_id = orderResult.id,
                billing_data = new
                {
                    apartment = "NA",
                    email = "email@test.com",
                    floor = "NA",
                    first_name = "User",
                    street = "NA",
                    building = "NA",
                    phone_number = "01000000000",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "Cairo",
                    country = "EG",
                    last_name = "User",
                    state = "Cairo"
                },
                currency = "EGP",
                integration_id = int.Parse(_config["Paymob:IntegrationId"])
            });
            var paymentKeyResult = await paymentKeyResponse.Content.ReadFromJsonAsync<PaymentKeyResponse>();

            // 4. Step: Return the iframe URL
            var iframeUrl = _config["Paymob:IframeUrl"] + paymentKeyResult.token;
            return iframeUrl;
        }
        public class AuthTokenResponse
        {
            public string token { get; set; }
        }

        public class OrderRegistrationResponse
        {
            public int id { get; set; }
        }

        public class PaymentKeyResponse
        {
            public string token { get; set; }
        }

    }

}
