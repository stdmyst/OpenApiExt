using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiExt._Internals.Extensions;
using OpenApiExt._Internals.XmlDocumentation;
using OpenApiExt.Models;

namespace OpenApiExt._Internals.Transformers;

/// <summary>
/// Changes enum type schemas.
/// </summary>
internal class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string ListElementToken = "- ";
    
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsEnum) 
            return Task.CompletedTask;
        
        schema.Type = GetSchemaType(type);
        schema.Enum ??= GetSchemaEnum(type);
        
        var schemaDescription = GenerateSchemaDescription(originalDescription: schema.Description, type);
        if (!string.IsNullOrEmpty(schemaDescription))
        {
            schema.Description = schemaDescription;
            SetXEnumDescriptionExtension(schema, schemaDescription);
        }

        return Task.CompletedTask;
    }

    private JsonSchemaType GetSchemaType(Type type)
        => type.HasJsonConverterAttribute<JsonStringEnumConverter>() 
            ? JsonSchemaType.String 
            : JsonSchemaType.Integer;

    private IList<JsonNode> GetSchemaEnum(Type type)
    {
        var jsonArray = type.HasJsonConverterAttribute<JsonStringEnumConverter>()
            ? GetEnumNames(type).ToJsonArray()
            : GetEnumValues(type).ToJsonArray();
        
        return jsonArray as IList<JsonNode>;
    }

    private string? GenerateSchemaDescription(string? originalDescription, Type type)
    {
        var enumFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        var enumNames = GetEnumNames(type);
        var enumValues = GetEnumValues(type);
        
        var sb = new StringBuilder();
        
        sb.Append(!string.IsNullOrWhiteSpace(originalDescription) 
            ? originalDescription 
            : type.Name);

        for (var i = 0; i < enumNames.Length; i++)
        {
            var name = enumNames[i];
            var field = enumFields.FirstOrDefault(f => f.Name == name);
            
            if (field == null)
                return originalDescription;
            
            var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description;
            
            AddNewLine(sb);
            AddEnumValueNameMapLine(sb, enumValues[i], name);
            
            // Use DescriptionAttribute value if presents.
            if (description != null)
                AddEnumElementDescription(sb, description);
            
            // Search summary in XML documentation.
            else if (XmlDocumentationProvider.TryGetFieldSummary(field, out var xmlSummary))
                AddEnumElementDescription(sb, xmlSummary!);
        }
        
        return sb.ToString();
    }

    private void SetXEnumDescriptionExtension(OpenApiSchema schema, string description)
    {
        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        schema.Extensions.Add(ExtensionKeys.XEnumDescriptionExtensionKey, new JsonNodeExtension(description));
    }

    private string[] GetEnumNames(Type type) 
        => Enum.GetNames(type);

    private List<int> GetEnumValues(Type type)
    {
        var result = new List<int>();
        foreach (var e in Enum.GetValues(type))
            result.Add(Convert.ToInt32(e));
        
        return result;
    }
    
    private void AddNewLine(StringBuilder sb) 
        => sb.Append(Consts.NewLine);
    
    private void AddEnumValueNameMapLine(StringBuilder sb, int value, string name) 
        => sb.Append($"{ListElementToken}{value} = {name}");
    
    private void AddEnumElementDescription(StringBuilder sb, string description) 
        => sb.Append($" ({description})");
}