using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class BankLinkService : BaseApiClient
    {
        private const string _baseUrl = "https://testdev.coralpay.com:5000/BankLinkService/api/";

        public BankLinkService(HttpClient httpClient) : base(httpClient)
        { }

        public async Task<InvokePaymentResponse> InvokePaymentAsync(InvokePaymentRequest request)
        {
            return await PostAsync<InvokePaymentResponse>($"{_baseUrl}InvokePayment", request);
        }

        public async Task<TransactionQueryResponse> QueryTransactionAsync(TransactionQueryRequest request)
        {
            return await PostAsync<TransactionQueryResponse>($"{_baseUrl}transactionQuery", request);
        }
    }
}


