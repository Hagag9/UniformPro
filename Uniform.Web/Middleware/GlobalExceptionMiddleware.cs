using Microsoft.AspNetCore.Diagnostics;

namespace UniformPro.Web.Middleware
{
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
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                
                // Check if request is for Admin area
                var path = context.Request.Path.Value ?? "";
                var isAdminRequest = path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);
                
                // Check if AJAX request
                var isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                
                if (isAjax)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { 
                        success = false, 
                        message = "حدث خطأ غير متوقع" 
                    });
                }
                else
                {
                    // Redirect to appropriate error controller
                    context.Response.Redirect(isAdminRequest ? "/Admin/Error/General" : "/Error/ServerError");
                }
            }
        }
    }
}
