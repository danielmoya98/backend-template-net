using Microsoft.AspNetCore.Http;

namespace BackendTemplate.Api.Middlewares;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevent Clickjacking for API responses
        headers["X-Frame-Options"] = "DENY";

        // Restrict referrer information sent with requests
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Disable unnecessary browser features
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        // Restrict Adobe Flash / PDF cross-domain policies
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Content Security Policy: Allow Scalar and Swagger UI while keeping API strict
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (!path.StartsWith("/swagger") && !path.StartsWith("/scalar") && !path.StartsWith("/openapi"))
        {
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none';";
        }

        await _next(context);
    }
}
