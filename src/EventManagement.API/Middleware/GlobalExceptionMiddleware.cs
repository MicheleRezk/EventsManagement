using System.Net;
using System.Text.Json;
using EventManagement.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred while processing the request. Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = new ProblemDetails();

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Failed";
                problemDetails.Detail = validationEx.Message;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                if (validationEx.Errors.Any())
                {
                    problemDetails.Extensions.Add("errors", validationEx.Errors);
                }
                break;
            
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Authentication Failed";
                problemDetails.Detail = exception.Message;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid Operation";
                problemDetails.Detail = exception.Message;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                break;
            
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = exception.Message;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An error occurred while processing your request. Please try again later.";
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                break;
        }

        problemDetails.Instance = context.Request.Path;

        var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}
