using System.Net;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Application.Models;

namespace MiniMart.API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected static ObjectResult CreateCustomResult<T>(HttpStatusCode statusCode, T? result, string errorMessage = "") where T : class
    {
        var response = new ApiResponse<T>
        {
            IsSuccessful = IsSuccessfulStatusCode(statusCode),
            StatusCode = (int)statusCode,
            ErrorMessage = errorMessage,
            Data = result
        };
        
        return new CustomObject<T>(response);
    }

    protected static ObjectResult CreateCustomResult(HttpStatusCode statusCode, string errorMessage = "")
    {
        var response = new ApiResponse<object>
        {
            IsSuccessful = IsSuccessfulStatusCode(statusCode),
            StatusCode = (int)statusCode,
            ErrorMessage = errorMessage
        };
        
        return new CustomObject<object>(response);
    }

    private static bool IsSuccessfulStatusCode(HttpStatusCode statusCode)
    {
        var num = Convert.ToInt32(statusCode);
        return num is >= 200 and <= 299;
    }
}

public class CustomObject<T>(ApiResponse<T> data) : ObjectResult(data);
