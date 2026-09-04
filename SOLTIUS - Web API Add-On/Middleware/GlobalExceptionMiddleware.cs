using System.Text.Json;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using SOLTIUS_Web_API_Add_On.Exceptions;

namespace SOLTIUS_Web_API_Add_On.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiNotConfiguredException ex)
            {
                _logger.LogWarning("API request rejected because configuration is missing.");
                await WriteResponse(context, StatusCodes.Status503ServiceUnavailable, "NOT_CONFIGURED", ex.Message);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, ex.Message);
                await WriteResponse(context, StatusCodes.Status500InternalServerError, "MYSQL_ERROR", ex.Message);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, ex.Message);
                await WriteResponse(context, StatusCodes.Status500InternalServerError, "SQLSERVER_ERROR", ex.Message);
            }
            catch (ApiConfigInvalidException ex)
            {
                _logger.LogWarning("API request rejected because configuration is invalid: {Message}", ex.Message);
                await WriteResponse(context, StatusCodes.Status400BadRequest, "INVALID_CONFIG", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing request.");

                // In Development: show detail. In Production: hide internal error.
                if (_env.IsDevelopment())
                    await WriteResponse(context, StatusCodes.Status500InternalServerError, ex.GetType().Name, ex.Message);
                else
                    await WriteResponse(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An internal server error occurred.");
            }
        }

        private static async Task WriteResponse(HttpContext context, int statusCode, string errorCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                errorCode,
                message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
