using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace OpenApiExt._Internals.XmlDocumentation;


internal static class XmlDocumentationProvider
{
    private static readonly MemoryCache XmlDocumentSummariesCache
        = new(new MemoryCacheOptions());
    
    public static bool TryGetFieldSummary(FieldInfo field, out string? summary)
    {
        summary = null;
        
        var type = field.DeclaringType;
        if (type is null) return false;
        
        var assembly = type.Assembly;
        var xmlSummaries = GetXmlSummaries(assembly);
        
        return xmlSummaries.TryGetValue(new FieldKey(type, field), out summary);
    }
    
    private static ReadOnlyDictionary<FieldKey, string> GetXmlSummaries(Assembly assembly)
    {
        Dictionary<FieldKey, string> xmlSummaries;
        
        if (XmlDocumentSummariesCache.TryGetValue(assembly, out Dictionary<FieldKey, string>? value) 
            && value != null)
        {
            xmlSummaries = value;
        }
        else
        {
            var xmlDocumentationFile = Path.ChangeExtension(assembly.Location, XmlDocumentationConsts.XmlExtension);
            xmlSummaries = GetXmlSummaries(xmlDocumentationFile);
            
            XmlDocumentSummariesCache.Set(assembly, xmlSummaries);
        }

        return new ReadOnlyDictionary<FieldKey, string>(xmlSummaries);
    }

    private static Dictionary<FieldKey, string> GetXmlSummaries(string xmlDocumentationFile)
    {
        var xmlSummaries = new Dictionary<FieldKey, string>();
        
        if (!File.Exists(xmlDocumentationFile))
            return xmlSummaries;

        var document = XDocument.Load(xmlDocumentationFile);
        var fields = GetFields(document);
        
        foreach (var field in fields)
        {
            var name = field.Attribute(XmlDocumentationConsts.NameAttribute)?.Value;
            var summary = field.Elements().FirstOrDefault()?.Value;
            
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(summary))
                continue;
            
            xmlSummaries.Add(new FieldKey(name), summary.Trim());
        }

        return xmlSummaries;
    }
    
    private static IEnumerable<XElement> GetFields(XDocument document) 
        => document.Descendants(XmlDocumentationConsts.MemberAttribute)
            .Where(m => m.Attribute(XmlDocumentationConsts.NameAttribute)
                ?.Value.Contains(XmlDocumentationConsts.FieldToken) ?? false);
}