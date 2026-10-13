using ShopSphere.Application.Common;
using ShopSphere.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace ShopSphere.API.Middleware
{
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
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                NotFoundException => HttpStatusCode.NotFound,          // 404
                ConflictException => HttpStatusCode.Conflict,          // 409
                ValidationException => HttpStatusCode.BadRequest,      // 400
                UnauthorizedException => HttpStatusCode.Unauthorized,  // 401
                _ => HttpStatusCode.InternalServerError                // 500 (anything unexpected)
            };

            var response = new ApiErrorResponse
            {
                StatusCode = (int)statusCode,
                Message = ex.Message,
                Details = statusCode == HttpStatusCode.InternalServerError
                    ? "An unexpected error occurred. Please try again later."
                    : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.StatusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, jsonOptions);
            return context.Response.WriteAsync(json);
        }
    }
}