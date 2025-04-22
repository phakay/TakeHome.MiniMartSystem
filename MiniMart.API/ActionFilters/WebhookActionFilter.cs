using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using MiniMart.Application.Models;

namespace MiniMart.API.ActionFilters
{
    public class WebhookActionFilter : ActionFilterAttribute
    {
        private string ApiKey;
        private string SecretHeaderName;

        public WebhookActionFilter(IOptions<BankLinkServiceConfig> serviceConfig)
        {
            SecretHeaderName = serviceConfig.Value.WebhookSecretHeader;
            ApiKey = serviceConfig.Value.WebhookSecretHeaderValue;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var reqHeaders = context.HttpContext.Request.Headers;
            if (!reqHeaders.ContainsKey(SecretHeaderName) || reqHeaders["SecretHeaderName"].ToString() != ApiKey)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authentication check failed" });
            }
        }
    }
}
