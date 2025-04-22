using Microsoft.Extensions.Options;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class PayWithTransferService : IExternalGatewayPaymentService
    {
        private readonly BankLinkService _apiService;
        private readonly BankLinkServiceConfig _config;

        public PayWithTransferService(BankLinkService apiService, IOptions<BankLinkServiceConfig> serviceConfig)
        {
            _apiService = apiService;
            _config = serviceConfig.Value;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {
            var requestPayload = new InvokePaymentRequest 
            {
                RequestHeader = new InvokePaymentRequest.Requestheader
                {
                    MerchantId = _config.MerchantId,
                    TerminalId = _config.TerminalId,
                    TraceId = Utility.GenerateTraceId()
                },

                Amount = paymentRequest.Amount,
                Description = $"Pay with transfer request for '{paymentRequest.CustomerId}'",
                AccountName = _config.AccountName
            };

            var responsePayload = await _apiService.InvokePaymentAsync(requestPayload);

            var responseHeader = responsePayload.ResponseHeader;
            if (responseHeader is null)
            {
                return new PaymentResponse { IsSuccessful = false, ErrorMessage = "No response data" };
            }

            if (responseHeader.ResponseCode == Constants.Successful)
            {
                var res = new PaymentResponse
                {
                    IsSuccessful = true,
                    Amount = paymentRequest.Amount,
                    ExpiryTime = responsePayload.ExpiryTime,
                    TrackingReference = responseHeader.TraceId,
                    BankName = responsePayload.DestinationBankName,
                    AccountName = responsePayload.DestinationAccountName,
                    AccountNumber = responsePayload.DestinationAccountNumber,
                    CurrencyCode = "NGN"
                };

                return res;
            }

            var badResponse = new PaymentResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"{Constants.Errors.GetValueOrDefault(responseHeader.ResponseCode, string.Empty)}" +
                               $"-{responseHeader.ResponseMessage}"
            };

            return badResponse;
        }

        public async Task<QueryTransactionResponse> QueryTransactionStatusAsync(QueryTransactionRequest request)
        {
            var requestPayload = new TransactionQueryRequest
            {
                RequestHeader = new TransactionQueryRequest.Requestheader
                {
                    MerchantId = _config.MerchantId,
                    TerminalId = _config.TerminalId,
                    TraceId = request.TransactionId
                }
            };

            var responsePayload = await _apiService.QueryTransactionAsync(requestPayload);

            var responseHeader = responsePayload.ResponseHeader;
            if (responseHeader is null)
            {
                return new QueryTransactionResponse { IsSuccessful = false, ErrorMessage = "No response data" };
            }

            if (responseHeader.ResponseCode == Constants.Successful && responsePayload.TransactionResponseCode == Constants.Successful)
            {
                var res = new QueryTransactionResponse
                {
                    IsSuccessful = true,
                    AmountProcessed = responsePayload.Amount
                };
                return res;
            }

            var badResponse = new QueryTransactionResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"{Constants.Errors.GetValueOrDefault(responseHeader.ResponseCode, string.Empty)}-" +
                               $"{responseHeader.ResponseMessage}-{responsePayload.TransactionResponseCode}-" +
                               $"{responsePayload.TransactionResponseMessage}",
                ShouldRequery = responsePayload.TransactionResponseCode == Constants.Pending_Transaction
            };

            return badResponse;
        }
    }
}
