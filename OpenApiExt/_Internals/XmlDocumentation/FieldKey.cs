using System.Reflection;

namespace OpenApiExt._Internals.XmlDocumentation;

internal record FieldKey
{
    public string Value { get; init; }
    
    public FieldKey(string value)
        => Value = value;
    
    public FieldKey(Type type, FieldInfo fieldInfo) 
        => Value = $"{XmlDocumentationConsts.FieldToken}{type.FullName}.{fieldInfo.Name}";
}