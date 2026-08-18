using System.ComponentModel;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OpenApiExt._Internals.Extensions;
using OpenApiExt._Internals.XmlDocumentation;
using OpenApiExt.Models;

namespace OpenApiExt._Internals.Transformers;

internal class EnumDescriptionTransformer : IOpenApiSchemaTransformer, IOpenApiOperationTransformer
{
    private const string ListElementToken = "- ";

    #region Operation

    /// <summary>
    /// Sets enum parameter descriptions using schema <see cref="ExtensionKeys.XEnumDescriptionExtensionKey"/> extension if presents.
    /// </summary>
    /// <remarks>
    /// If a description is already present, for example if the value of an XML param element is specified, it is not overridden.
    /// </remarks>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var parameters = operation.Parameters;
        if (parameters is null) return Task.CompletedTask;
        
        foreach (var parameter in parameters)
        {
            if (parameter.Description is null 
                && parameter.Schema?.Extensions is not null
                && parameter.Schema.Extensions.TryGetValue(ExtensionKeys.XEnumDescriptionExtensionKey, out var value)
                && value is JsonNodeExtension xEnumDescriptionExtension)
            {
                parameter.Description = xEnumDescriptionExtension.Node.ToString();
            }
        }
        
        return Task.CompletedTask;
    }

    #endregion

    #region Schema

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsEnum) return Task.CompletedTask;
        
        var schemaDescription = GenerateSchemaDescription(originalDescription: schema.Description, type);
        if (!string.IsNullOrEmpty(schemaDescription))
        {
            schema.Description = schemaDescription;
            SetXEnumDescriptionExtension(schema, schemaDescription);
        }

        return Task.CompletedTask;
    }

    private static string? GenerateSchemaDescription(string? originalDescription, Type type)
    {
        var enumFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        var enumNames = TypesExtensions.GetEnumNames(type);
        var enumValues = TypesExtensions.GetEnumValues(type);
        
        var sb = new StringBuilder();
        
        sb.Append(!string.IsNullOrWhiteSpace(originalDescription) 
            ? originalDescription 
            : type.Name);

        for (var i = 0; i < enumNames.Count; i++)
        {
            var name = enumNames[i];
            
            var field = enumFields.FirstOrDefault(f => f.Name == name);
            
            if (field == null) return originalDescription;
            
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

    private static void SetXEnumDescriptionExtension(OpenApiSchema schema, string description)
    {
        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        schema.Extensions.Add(ExtensionKeys.XEnumDescriptionExtensionKey, new JsonNodeExtension(description));
    }
    
    private static void AddNewLine(StringBuilder sb) 
        => sb.Append(Consts.NewLine);
    
    private static void AddEnumValueNameMapLine(StringBuilder sb, long value, string name) 
        => sb.Append($"{ListElementToken}{value} = {name}");
    
    private static void AddEnumElementDescription(StringBuilder sb, string description) 
        => sb.Append($" ({description})");

    #endregion
}