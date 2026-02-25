using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OpenApiExt.Transformers;

/// <summary>
/// Updates enum parameters descriptions using schema <see cref="Consts.XEnumDescriptionExtensionKey"/> extension if presents.
/// </summary>
/// <remarks>It will be used value from param XML element if presents instead.</remarks>
public class EnumParameterDescriptionTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var parameters = operation.Parameters;
        if (parameters is null) return Task.CompletedTask;
        
        foreach (var parameter in parameters)
        {
            if (parameter.Description is null 
                && parameter.Schema?.Extensions is not null
                && parameter.Schema.Extensions.TryGetValue(Consts.XEnumDescriptionExtensionKey, out var value)
                && value is JsonNodeExtension xEnumDescriptionExtension)
            {
                parameter.Description = xEnumDescriptionExtension.Node.ToString();
            }
        }
        
        return Task.CompletedTask;
    }
}