using MiniMart.Application.Models;

namespace MiniMart.API.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.StatusCode = 500;
            var response = new ApiResponse<string>
            {
                IsSuccessful = false,
                StatusCode = 500,
                ErrorMessage = "An unhandled exception occurred."
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}