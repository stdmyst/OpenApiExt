using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiExt._Internals.Transformers;

namespace OpenApiExt;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions AddBearerAuth(this OpenApiOptions options) 
        => options.AddDocumentTransformer<BearerAuthDocumentTransformer>();

    public static OpenApiOptions AddEnumDescriptionSupport(this OpenApiOptions options)
    {
        options.AddSchemaTransformer<EnumSchemaTransformer>();
        options.AddOperationTransformer<EnumParameterDescriptionTransformer>();

        return options;
    }

    public static OpenApiOptions AddAuthResponses(this OpenApiOptions options, bool showRequiredRoles) 
        => options.AddOperationTransformer((operation, context, cancellationToken) => 
            new ResponsesAuthTransformer(showRequiredRoles).TransformAsync(operation, context, cancellationToken));

    public static OpenApiOptions UseFullNameOfTypes(this OpenApiOptions options)
    {
        options.CreateSchemaReferenceId = info =>
        {
            var type = info.Type;
            
            var referenceId = OpenApiOptions.CreateDefaultSchemaReferenceId(info)
                ?.Replace(type.Name, type.FullName);

            return referenceId;
        };
        
        return options;
    }
    
    /// <summary>
    /// Clear server objects from OpenApi document and set one with relative URL.<br/>
    /// All paths should be interpreted as relative.
    /// </summary>
    /// <remarks>
    /// It suitable in cases when using proxy.<br/>
    /// It is also possible to use HTTP Forwarded Headers instead to apply correct headers processing through UseForwardedHeaders middleware.
    /// </remarks>
    public static OpenApiOptions UseRelativeServerUrl(this OpenApiOptions options)
        => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Servers = new List<OpenApiServer> { new() { Url = "/" } };
            
            return Task.CompletedTask;
        });
}