using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class FakePayWithTransferService : IExternalGatewayPaymentService
    {
        private static Stack<int> _store = [];

        private static void RefillStore() => Enumerable.Range(1, 10).ToList().ForEach(x => _store.Push(x));

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {
            if (_store.Count == 0) RefillStore();

            await Task.Delay(2000);

            if (_store.Pop() % 5 == 0)
            {
                return new PaymentResponse
                {
                    IsSuccessful = true,
                    AccountName = "Test Account Name",
                    AccountNumber = "10001001",
                    ExpiryTime = DateTime.Now.AddSeconds(30),
                    CurrencyCode = "NGN",
                    Amount = paymentRequest.Amount,
                    BankName = "Test Bank",
                    TrackingReference = Utility.GenerateTraceId()
                };
            }
            else
            {
                return new PaymentResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "Unable to reach gateway"
                };
            }
        }

        public async Task<QueryTransactionResponse> QueryTransactionStatusAsync(QueryTransactionRequest request)
        {
            if (_store.Count == 0) RefillStore();

            await Task.Delay(2000);

            if (_store.Pop() % 10 == 0)
            {
                return new QueryTransactionResponse
                {
                    IsSuccessful = true,
                    AmountProcessed = 2000,
                    ShouldRequery = false
                };
            }
            else if (_store.Pop() % 5 == 0)
            {
                return new QueryTransactionResponse
                {
                    IsSuccessful = false,
                    ShouldRequery = true,
                    ErrorMessage = "Pending"
                };
            }
            else
            {
                return new QueryTransactionResponse
                {
                    IsSuccessful = false,
                    ShouldRequery = false,
                    ErrorMessage = "Expired"
                };
            }
        }
    }
}
