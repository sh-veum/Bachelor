using System.Reflection;

namespace NetBackend.Tools;

public static class ReflectionHelper
{
    public static ClassInfo GetClassInfo<T>()
    {
        Type type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              .Where(p => !Attribute.IsDefined(p, typeof(GraphQLIgnoreAttribute))) // Exclude properties with the GraphQLIgnore attribute
                              .Select(p => new PropertyInfo
                              {
                                  Name = p.Name,
                                  PropertyType = p.PropertyType.Name
                              })
                              .ToList();

        return new ClassInfo
        {
            Name = type.Name,
            Properties = properties
        };
    }
}

public class ClassInfo
{
    public required string Name { get; set; }
    public required List<PropertyInfo> Properties { get; set; }
}

public class PropertyInfo
{
    public required string Name { get; set; }
    public required string PropertyType { get; set; }
}