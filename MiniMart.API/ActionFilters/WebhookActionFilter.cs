using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MiniMart.API.ActionFilters
{
    public class WebhookActionFilter : ActionFilterAttribute
    {
        private const string ApiKey = "api-key-from-gtw-service";
        private const string SecretHeaderName = "Secret";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var reqHeaders = context.HttpContext.Request.Headers;
            if (!reqHeaders.ContainsKey(SecretHeaderName))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authentication check failed" });
            }
        }
    }
}
