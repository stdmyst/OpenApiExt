using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OpenApiExt.Extensions;

public static class TypesExtensions
{
    public static bool HasJsonConverterAttribute<T>(this Type type) where T : JsonConverter 
        => type.GetCustomAttributes(false)
                .OfType<JsonConverterAttribute>()
                .FirstOrDefault(converter => converter.ConverterType == typeof(T)) 
            is not null;

    public static JsonArray ToJsonArray<T>(this IEnumerable<T> elements)
    {
        var jsonArray = new JsonArray();
        foreach (var element in elements)
            jsonArray.Add(element);

        return jsonArray;
    }
}