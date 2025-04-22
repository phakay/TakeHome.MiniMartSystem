using Microsoft.Extensions.Options;
using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class BankLinkService : BaseApiClient
    {
        private readonly BankLinkServiceConfig _serviceConfig;

        public BankLinkService(HttpClient httpClient, IOptions<BankLinkServiceConfig> serviceConfig) : base(httpClient)
        {
            ArgumentNullException.ThrowIfNull(serviceConfig);
            _serviceConfig = serviceConfig.Value;
        }

        public async Task<InvokePaymentResponse> InvokePaymentAsync(InvokePaymentRequest request)
        {
            return await PostAsync<InvokePaymentResponse>($"{_serviceConfig.BaseUrl}{_serviceConfig.InvokePaymentendpoint}", request);
        }

        public async Task<TransactionQueryResponse> QueryTransactionAsync(TransactionQueryRequest request)
        {
            return await PostAsync<TransactionQueryResponse>($"{_serviceConfig.BaseUrl}{_serviceConfig.TransactionQueryEndpoint}", request);
        }
    }
}


