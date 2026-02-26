using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiExt._Internals.Extensions;

namespace OpenApiExt._Internals.Transformers;

internal class BearerAuthDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Add the security scheme at the document level.
        (string Key, OpenApiSecurityScheme Value) securityScheme = (Consts.BearerSchemeReferenceId, new OpenApiSecurityScheme 
        {
            Type = SecuritySchemeType.Http,
            Scheme = Consts.BearerScheme,
            In = ParameterLocation.Header,
            BearerFormat = Consts.JwtBearerFormat,
            Description = "Bearer authorization. Example: \"Authorization: Bearer {token}\""
        });
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.AddOrUpdate(securityScheme.Key, securityScheme.Value);
        
        // Add the security requirement at the document level.
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(securityScheme.Key, document)] = []
        });
        
        return Task.CompletedTask;
    }
}