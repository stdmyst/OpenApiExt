using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OpenApiExt._Internals.Transformers;

internal class BearerAuthDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Add the security scheme at the document level.
        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Description = "Bearer authorization. Example: \"Authorization: Bearer {token}\""
            }
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = securitySchemes;

        // Apply it as a requirement for each operation.
        foreach (var operation in document.Paths.Values
                     .Where(path => path.Operations is not null)
                     .SelectMany(path => path.Operations!))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
        
        return Task.CompletedTask;
    }
}