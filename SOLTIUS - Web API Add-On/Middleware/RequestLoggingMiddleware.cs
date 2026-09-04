using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SOLTIUS_Web_API_Add_On.Services.AuditLog;

namespace SOLTIUS_Web_API_Add_On.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuditLogService _auditLog;

        private static readonly string[] SkipPaths = new[]
        {
            "/swagger",
            "/favicon.ico",
            "/health"
        };

        public RequestLoggingMiddleware(RequestDelegate next, IAuditLogService auditLog)
        {
            _next = next;
            _auditLog = auditLog;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path + context.Request.QueryString;

            // Skip logging for internal paths
            if (ShouldSkip(path))
            {
                await _next(context);
                return;
            }

            string method = context.Request.Method;
            string clientIp = context.Connection?.RemoteIpAddress?.ToString() ?? "";

            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                int status = context.Response.StatusCode;
                string detail = $"duration={sw.ElapsedMilliseconds}ms";
                _auditLog.LogApiRequest(method, path, status, clientIp, "", detail, (int)sw.ElapsedMilliseconds);
            }
        }

        private static bool ShouldSkip(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            foreach (var skip in SkipPaths)
            {
                if (path.StartsWith(skip, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
