using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BackendTemplate.Api.OpenApi;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;
    }

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = securityScheme;

            var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);
            var requirement = new OpenApiSecurityRequirement
            {
                [schemeReference] = new List<string>()
            };

            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(requirement);
        }
    }
}
