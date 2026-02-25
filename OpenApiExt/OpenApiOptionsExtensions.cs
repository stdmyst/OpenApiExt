using Microsoft.AspNetCore.OpenApi;
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
}