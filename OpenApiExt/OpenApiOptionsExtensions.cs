using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiExt._Internals.Transformers;
using OpenApiExt.Models;

namespace OpenApiExt;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions AddDocumentInfo(this OpenApiOptions options,
        string documentVersion = "v1",
        string? documentTitle = null) 
        => options.AddDocumentTransformer((document, context, cancellationToken) => 
            new DocumentInfoTransformer(documentVersion, documentTitle).TransformAsync(document, context, cancellationToken));

    public static OpenApiOptions AddBearerAuth(this OpenApiOptions options)
        => options.AddDocumentTransformer<BearerAuthDocumentTransformer>();

    public static OpenApiOptions AddAuthResponses(this OpenApiOptions options, bool showRequiredRoles)
        => options.AddOperationTransformer((operation, context, cancellationToken) =>
            new ResponsesAuthTransformer(showRequiredRoles).TransformAsync(operation, context, cancellationToken));

    public static OpenApiOptions AddEnumDescriptionSupport(this OpenApiOptions options)
    {
        options.AddSchemaTransformer<EnumSchemaTransformer>();
        options.AddOperationTransformer<EnumParameterDescriptionTransformer>();

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

    public static OpenApiOptions ConfigureSchemaReferenceIdGeneration(this OpenApiOptions openApiOptions, SchemaReferenceIdGenerationOptions schemaGenerationOptions)
    {
        openApiOptions.CreateSchemaReferenceId = jsonTypeInfo =>
        {
            var type = jsonTypeInfo.Type;

            // NOTE: A null value indicates that the schema should be inlined.
            if (schemaGenerationOptions.UseInlineEnums && type.IsEnum)
                return null;
            
            return schemaGenerationOptions.UseFullTypeNames 
                ? GetFullTypeNameReferenceId(jsonTypeInfo) 
                : OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
        };

        return openApiOptions;
    }
    
    private static string? GetFullTypeNameReferenceId(JsonTypeInfo jsonTypeInfo)
    {
        var type = jsonTypeInfo.Type;
        
        var defaultReferenceId = OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
        if (defaultReferenceId is null)
            return null;

        string referenceId;

        if (type.IsGenericType && (type.FullName is not null || type.Namespace is not null))
        {
            // Uses fullname to handle nested generic types.
            var fullName = type.FullName;
            var pathToType = fullName?[..fullName.IndexOf('`')] ?? type.Namespace;

            referenceId = $"{pathToType}.{defaultReferenceId}";
        }
        else
        {
            referenceId = type.FullName ?? type.Name;
        }

        if (type.IsNested)
        {
            // Replaces the '+' symbol used to mark nested types.
            referenceId = referenceId.Replace('+', '.');
        }

        return referenceId;
    }
}