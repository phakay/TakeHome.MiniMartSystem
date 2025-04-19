using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.Application.Services
{
    public class FakePayWithTransferService : IExternalGatewayPaymentService
    {
        private HttpClient _httpClient;
        private string _payentGetewayUrl = "https://api.pay.com";
        
        // For test
        private static int _queryCount = 0;

        public FakePayWithTransferService(HttpClient client)
        {
             _httpClient = client;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {
            // call paywith transfer endpoint to get transfer details to send back to client

            var response = new PaymentResponse()
            {
                AccountNumber = "10001",
                Amount = paymentRequest.Amount,
                CurrencyCode = "NGN",
                IsSuccess = true,
                ExpiryTime = DateTime.Now.AddSeconds(1000).ToLongDateString()
            };

            await Task.Delay(2000);

            return response;
        }

        public async Task<QueryTransactionResponse> QueryTransactionStatusAsync(QueryTransactionRequest request)
        {
            var response = new QueryTransactionResponse()
            {
                isPending = true
            };

            if (_queryCount > 0 && _queryCount % 3 == 0)
            {
                response = new QueryTransactionResponse() { isFailed = true };
            }

            await Task.Delay(2000);

            _queryCount++;

            return response;
        }
    }
}
