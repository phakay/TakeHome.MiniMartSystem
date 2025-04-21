using Microsoft.AspNetCore.Mvc;
using MiniMart.Application.Models;

namespace MiniMart.API.Models;

public class CustomObject<T>(ApiResponse<T> data) : ObjectResult(data);