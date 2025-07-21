using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotesApp.Domain.Exceptions;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NotesApp.WebApi.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var result = exception switch
        {
            ResourceNotFoundException ex => (
                HttpStatusCode.NotFound, (object)new
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ex.ErrorCode,
                    Message = ex.Message,
                    ex.ResourceType,
                    ex.Identifier,
                    ex.Timestamp
                }
            ),
            InvalidInputException ex => (
                HttpStatusCode.BadRequest, (object)new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ex.ErrorCode,
                    Message = ex.Message,
                    ex.Timestamp
                }
            ),
            NotesApp.Domain.Exceptions.UnauthorizedAccessException ex => (
                HttpStatusCode.Unauthorized, (object)new
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    ex.ErrorCode,
                    Message = ex.Message,
                    ex.Timestamp
                }
            ),
            ValidationException ex => (
                HttpStatusCode.BadRequest, (object)new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ex.ErrorCode,
                    Message = ex.Message,
                    Errors = ex.Errors,
                    ex.Timestamp
                }
            ),
            DuplicateResourceException ex => (
                HttpStatusCode.Conflict, (object)new
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ex.ErrorCode,
                    Message = ex.Message,
                    ex.ResourceType,
                    ex.Identifier,
                    ex.Timestamp
                }
            ),
            ResourceInUseException ex => (
                HttpStatusCode.Conflict, (object)new
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    ex.ErrorCode,
                    Message = ex.Message,
                    ex.ResourceType,
                    ex.Identifier,
                    AssociatedCount = ex.AssociatedCount,
                    ex.Timestamp
                }
            ),
            ConfigurationException ex => (
                HttpStatusCode.InternalServerError, (object)new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ex.ErrorCode,
                    Message = ex.Message,
                    ex.Timestamp
                }
            ),
            FileOperationException ex => (
                HttpStatusCode.BadRequest, (object)new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ex.ErrorCode,
                    Message = ex.Message,
                    ex.OperationType,
                    ex.FileName,
                    ex.Timestamp
                }
            ),
            ArgumentNullException ex => (
                HttpStatusCode.BadRequest, (object)new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "ArgumentNull",
                    Message = "Required parameter is missing",
                    Detail = ex.Message,
                    Timestamp = DateTime.UtcNow
                }
            ),
            ArgumentException ex => (
                HttpStatusCode.BadRequest, (object)new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ErrorCode = "ArgumentInvalid",
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError, (object)new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ErrorCode = "InternalServerError",
                    Message = "An unexpected error occurred",
                    Detail = exception.Message,
                    Timestamp = DateTime.UtcNow
                }
            )
        };

        var (statusCode, response) = result;

        _logger.LogError(exception, "Error occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}