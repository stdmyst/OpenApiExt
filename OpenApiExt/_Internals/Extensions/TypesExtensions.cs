namespace OpenApiExt._Internals.Extensions;

internal static class TypesExtensions
{
    extension(Type type)
    {
        public List<string> GetEnumNames()
        {
            if (!type.IsEnum) throw new ArgumentException($"{type} must be an enum.");
        
            var names = Enum.GetNames(type);
        
            return names.ToList();
        }

        public List<long> GetEnumValues()
        {
            if (!type.IsEnum) throw new ArgumentException($"{type} must be an enum.");
        
            var values = Enum.GetValues(type).Cast<object>().Select(Convert.ToInt64);
        
            return values.ToList();
        }
    }
    
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, TValue value)
    {
        collection.Remove(key);
        collection.Add(key, value);
    }
}