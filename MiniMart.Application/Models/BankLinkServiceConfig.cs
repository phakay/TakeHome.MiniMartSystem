namespace MiniMart.Application.Models
{
    public class BankLinkServiceConfig
    {
        public string ApiToken { get; set; }
        public string BaseUrl { get; set; }
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        public string AccountName { get; set; }
        public string WebhookSecretHeader { get; set; }
        public string WebhookSecretHeaderValue { get; set; }
        public string InvokePaymentendpoint { get; set; }
        public string TransactionQueryEndpoint { get; set; }
    }
}
