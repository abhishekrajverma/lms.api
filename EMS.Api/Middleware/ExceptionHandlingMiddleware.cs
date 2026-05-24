namespace EMS.Api.Middleware;

using EMS.Shared.Common;
using EMS.Shared.Exceptions;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;

/// <summary>
/// Global exception handling middleware
/// Catches all unhandled exceptions and returns standardized error responses
/// </summary>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>();

        switch (exception)
        {
            case NotFoundException notFound:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = ApiResponse<object>.ErrorResponse(notFound.Message, 404);
                break;

            case BadRequestException badRequest:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ApiResponse<object>
                {
                    Success = false,
                    Message = badRequest.Message,
                    Data = badRequest.Errors,
                    StatusCode = 400
                };
                break;

            case UnauthorizedException unauthorized:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response = ApiResponse<object>.ErrorResponse(unauthorized.Message, 401);
                break;

            case ForbiddenException forbidden:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                response = ApiResponse<object>.ErrorResponse(forbidden.Message, 403);
                break;

            case ConflictException conflict:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response = ApiResponse<object>.ErrorResponse(conflict.Message, 409);
                break;

            case UnprocessableEntityException unprocessable:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response = ApiResponse<object>.ErrorResponse(unprocessable.Message, 422);
                break;

            case InternalServerException internalServer:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = ApiResponse<object>.ErrorResponse(internalServer.Message, 500);
                break;

            case ServiceUnavailableException serviceUnavailable:
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                response = ApiResponse<object>.ErrorResponse(serviceUnavailable.Message, 503);
                break;

            case RequestTimeoutException timeout:
                context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                response = ApiResponse<object>.ErrorResponse(timeout.Message, 408);
                break;

            case SqlException sqlEx:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                // Log full error internally
                //_logger.LogError(sqlEx, "Database error occurred. Number: {Number}", sqlEx.Number);

                response = ApiResponse<object>.ErrorResponse(
                    "A database error occurred while processing your request.",
                    500);
                break;


            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = ApiResponse<object>.ErrorResponse(
                    "An unexpected error occurred. Please try again later.",
                    500);
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}
