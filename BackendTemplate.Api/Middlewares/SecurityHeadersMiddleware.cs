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

        // Previene que el navegador "adivine" el tipo de contenido y ejecute scripts ocultos
        headers["X-Content-Type-Options"] = "nosniff";
        
        // Previene ataques XSS básicos
        headers["X-XSS-Protection"] = "1; mode=block";
        
        // Evita que la API sea incrustada en un iFrame (Clickjacking)
        headers["X-Frame-Options"] = "DENY";
        
        // Política de seguridad de contenido restrictiva para una API
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; sandbox";

        await _next(context);
    }
}
