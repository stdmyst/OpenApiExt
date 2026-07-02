namespace OpenApiExt.Models;

public record SchemaReferenceIdGenerationOptions
{
    public bool UseInlineEnums { get; init; }
        
    public bool UseFullTypeNames { get; init; }
}