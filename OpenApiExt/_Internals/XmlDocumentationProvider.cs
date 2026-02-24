using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace OpenApiExt._Internals;

internal static class XmlDocumentationProvider
{
    private const string FieldToken = "F:";
    private const string NameAttribute = "name";
    private const string MemberAttribute = "member";
    private const string XmlExtension = ".xml";
    
    private static readonly MemoryCache XmlDocumentSummariesCache
        = new(new MemoryCacheOptions());

    public static ReadOnlyDictionary<string, string> GetXmlSummaries(Type type) 
        => GetXmlSummaries(type.Assembly);

    public static bool TryGetFieldSummary(FieldInfo field, out string? summary)
    {
        summary = null;
        var type = field.DeclaringType;
        if (type is null) return false;
        
        var assembly = type.Assembly;
        var xmlSummaries = GetXmlSummaries(assembly);
        
        return xmlSummaries.TryGetValue(GetXmlFieldString(type, field), out summary);
    }
    
    private static ReadOnlyDictionary<string, string> GetXmlSummaries(Assembly assembly)
    {
        Dictionary<string, string> xmlSummaries;
        
        if (XmlDocumentSummariesCache.TryGetValue(assembly, out Dictionary<string, string>? value) 
            && value != null)
        {
            xmlSummaries = value;
        }
        else
        {
            var xmlDocumentationFile = Path.ChangeExtension(assembly.Location, XmlExtension);
            xmlSummaries = GetXmlSummaries(xmlDocumentationFile);
            XmlDocumentSummariesCache.Set(assembly, xmlSummaries);
        }

        return new ReadOnlyDictionary<string, string>(xmlSummaries);
    }

    private static Dictionary<string, string> GetXmlSummaries(string xmlDocumentationFile)
    {
        var xmlSummaries = new Dictionary<string, string>();
        
        if (!File.Exists(xmlDocumentationFile))
            return xmlSummaries;

        var document = XDocument.Load(xmlDocumentationFile);
        var members = GetFields(document);
        
        foreach (var member in members)
        {
            var name = member.Attribute(NameAttribute)?.Value;
            var summary = member.Elements().FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(summary))
                continue;

            xmlSummaries.Add(name, summary.Trim());
        }

        return xmlSummaries;
    }
    
    private static IEnumerable<XElement> GetFields(XDocument document) 
        => document.Descendants(MemberAttribute)
            .Where(m => m.Attribute(NameAttribute)?.Value.Contains(FieldToken) ?? false);

    private static string GetXmlFieldString(Type type, FieldInfo field) 
        => $"{FieldToken}{type.FullName}.{field.Name}";
}