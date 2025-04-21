using System.Security.Cryptography;
using MiniMart.Application.Contracts;
using MiniMart.Application.Models;

namespace MiniMart.Infrastructure.Services
{
    public class PayWithTransferService : IExternalGatewayPaymentService
    {
        private readonly BankLinkService _apiService;
        private string MerchantId = "Chuks12";
        private string TerminalId = "Chuks123";
        private string AccountName = "John Black";

        public PayWithTransferService(BankLinkService apiService)
        {
            _apiService = apiService;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {
            var requestPayload = new InvokePaymentRequest 
            {
                RequestHeader = new InvokePaymentRequest.Requestheader
                {
                    MerchantId = MerchantId,
                    TerminalId = TerminalId,
                    TraceId = Utility.GenerateTraceId()
                },

                Amount = paymentRequest.Amount,
                Description = $"Pay with transfer request for '{paymentRequest.CustomerId}'",
                AccountName = AccountName
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
                    MerchantId = MerchantId,
                    TerminalId = TerminalId,
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
