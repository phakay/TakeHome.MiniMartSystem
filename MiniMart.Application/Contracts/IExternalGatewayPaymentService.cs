using MiniMart.Application.Models;

namespace MiniMart.Application.Contracts
{
    public interface IExternalGatewayPaymentService
    {
        public Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest);

        public Task<QueryTransactionResponse> QueryTransactionStatusAsync(QueryTransactionRequest request);
    }
}
