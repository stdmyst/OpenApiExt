using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OpenApiExt._Internals.Extensions;

internal static class TypesExtensions
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

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, TValue value)
    {
        collection.Remove(key);
        collection.Add(key, value);
    }
}