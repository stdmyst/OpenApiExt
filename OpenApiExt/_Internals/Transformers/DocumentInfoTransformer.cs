using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OpenApiExt._Internals.Transformers;

public class DocumentInfoTransformer(string documentVersion = "v1", string? documentTitle = null) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info.Title = documentTitle ?? (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly())
            .GetName().Name;
                    
        document.Info.Version = documentVersion;
                    
        return Task.CompletedTask;
    }
}