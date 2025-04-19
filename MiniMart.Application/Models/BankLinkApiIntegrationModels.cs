using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMart.Application.Models
{
    public class TransactionQueryRequest
    {
        public Requestheader RequestHeader { get; set; }

        public class Requestheader
        {
            public string MerchantId { get; set; }
            public string TerminalId { get; set; }
            public string TraceId { get; set; }
        }
    }

    public class TransactionQueryResponse
    {
        public Responseheader ResponseHeader { get; set; }
        public string TransactionResponseCode { get; set; }
        public string TransactionResponseMessage { get; set; }
        public string SourceBankCode { get; set; }
        public string SourceAccountName { get; set; }
        public string SourceBankName { get; set; }
        public string SourceBankAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string DestinationBankCode { get; set; }
        public string DestinationBankName { get; set; }
        public string DestinationAccountName { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string TraceId { get; set; }
        public string TransactionId { get; set; }
        public string SessionId { get; set; }
        public string Terminal { get; set; }
        public string Description { get; set; }
        public float PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }

        public class Responseheader
        {
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
        }
    }

    public class InvokePaymentRequest
    {
        public Requestheader RequestHeader { get; set; }
        public decimal Amount { get; set; }
        public string? BankCode { get; set; }
        public string Description { get; set; }
        public string AccountName { get; set; }

        public class Requestheader
        {
            public string MerchantId { get; set; }
            public string TerminalId { get; set; }
            public string TraceId { get; set; }
        }
    }

    public class InvokePaymentResponse
    {
        public Responseheader ResponseHeader { get; set; }
        public string DestinationBankCode { get; set; }
        public string DestinationBankName { get; set; }
        public string Description { get; set; }
        public string DestinationAccountName { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string TransactionId { get; set; }
        public string SessionId { get; set; }
        public string DestinationBankLogo { get; set; }
        public DateTime ExpiryTime { get; set; }

        public class Responseheader
        {
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
            public string traceId { get; set; }
        }
    }

    public class CallbackRequest
    {
        public string TransactionResponseCode { get; set; }
        public string TransactionResponseMessage { get; set; }
        public string SourceBankCode { get; set; }
        public string SourceAccountName { get; set; }
        public string SourceBankName { get; set; }
        public string SourceBankAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string DestinationBankCode { get; set; }
        public string DestinationBankName { get; set; }
        public string DestinationAccountName { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string TraceId { get; set; }
        public string TransactionId { get; set; }
        public string SessionId { get; set; }
        public string Terminal { get; set; }
        public string Description { get; set; }
        public string MerchantId { get; set; }
    }

    public static class Constants
    {
        public const string Successful = "00";
        public const string Failed_Transaction_or_Request = "01";
        public const string Pending_Transaction = "09";
        public const string Mismatch_Transaction = "76";
        public const string Unable_To_Locate_Record = "25";
        public const string Expired_Account_Number = "54";

        public static readonly IReadOnlyDictionary<string, string> Errors =
            new Dictionary<string, string>
            {
                { Successful, "Successful" },
                { Failed_Transaction_or_Request, "Failed Transaction or Check Description Message" },
                { Pending_Transaction, "Pending Transaction" },
                { Mismatch_Transaction, "Mismatch Transaction" },
                { Unable_To_Locate_Record, "Unable to locate record" },
                { Expired_Account_Number, "Expired Account Number" }
            };
    }
}
