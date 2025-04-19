using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class PayWithTransferService : IExternalGatewayPaymentService
    {
        private readonly BankLinkService _apiService;
        public PayWithTransferService(BankLinkService apiService)
        {
            _apiService = apiService;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {
            var requestPayload = new InvokePaymentRequest { };

            var requestResponse = await _apiService.InvokePaymentAsync(requestPayload);

            var response = new PaymentResponse { };

            return response;
        }

        public async Task<QueryTransactionResponse> QueryTransactionStatusAsync(QueryTransactionRequest request)
        {
            var requestPayload = new TransactionQueryRequest { };

            var requestResponse = await _apiService.QueryTransactionAsync(requestPayload);

            var response = new QueryTransactionResponse { };

            return response;
        }
    }
}
