using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiExt._Internals;
using OpenApiExt.Extensions;

namespace OpenApiExt.Transformers;

/// <summary>
/// Changes enum type schemas.
/// </summary>
public class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsEnum) return Task.CompletedTask;

        SpecifySchemaType(schema, type);

        if (schema.Enum is null) 
            SetEnum(schema, type);
        
        SetDescription(schema, type);

        return Task.CompletedTask;
    }

    private void SpecifySchemaType(OpenApiSchema schema, Type type)
    {
        schema.Type = type.HasJsonConverterAttribute<JsonStringEnumConverter>() 
            ? JsonSchemaType.String 
            : JsonSchemaType.Integer;
    }

    private void SetEnum(OpenApiSchema schema, Type type)
    {
        var jsonArray = type.HasJsonConverterAttribute<JsonStringEnumConverter>()
            ? Enum.GetNames(type).ToJsonArray()
            : GetEnumValues(type).ToJsonArray();
        
        schema.Enum = jsonArray!;
    }

    private List<int> GetEnumValues(Type type)
    {
        var result = new List<int>();
        foreach (var e in Enum.GetValues(type))
            result.Add(Convert.ToInt32(e));
        
        return result;
    }

    private void SetDescription(OpenApiSchema schema, Type type)
    {
        var sb = new StringBuilder();
        
        if (!string.IsNullOrWhiteSpace(schema.Description))
            sb.Append(schema.Description);
        
        var enumFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        var enumNames = Enum.GetNames(type);
        var enumValues = GetEnumValues(type);
        
        for (var i = 0;  i < enumNames.Length; i++)
        {
            var name = enumNames[i];
            var field = enumFields.FirstOrDefault(f => f.Name == name);
            var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (field == null) return;
            
            if (description != null)
                sb.Append(Consts.NewLine + $"- {enumValues[i]} = {name} {Environment.NewLine}*{description}*");
            else if (XmlDocumentationProvider.TryGetFieldSummary(field, out var xmlSummary))
                sb.Append(Consts.NewLine + $"- {enumValues[i]} = {name} {Environment.NewLine}*{xmlSummary}*");
            else return;
        }
        
        schema.Description = sb.ToString();
        
        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        schema.Extensions.Add(Consts.XEnumDescriptionExtensionKey, new JsonNodeExtension(schema.Description ));
    }
}