using System.Net;
using System.Text.Json;
using BrowserAgent.Api.Domain.Exceptions;

namespace BrowserAgent.Api.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteErrorResponse(context, ex.Code, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await WriteErrorResponse(context, ex.Code, ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain rule violation: {Code}", ex.Code);

            context.Response.StatusCode = ex.Code switch
            {
                "EMAIL_EXISTS" or "DUPLICATE_DOCUMENT" or "MAX_PROVIDERS" => (int)HttpStatusCode.Conflict,
                "FILE_REQUIRED" or "FILE_TOO_LARGE" or "INVALID_FILE_TYPE" or "INVALID_MIME_TYPE" or "INVALID_DOCUMENT_TYPE" => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.UnprocessableEntity
            };

            await WriteErrorResponse(context, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteErrorResponse(context, "INTERNAL_ERROR", "An unexpected error occurred");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, string code, string message)
    {
        context.Response.ContentType = "application/json";
        var response = ApiResponse<object>.Fail(code, message);
        await JsonSerializer.SerializeAsync(context.Response.Body, response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
