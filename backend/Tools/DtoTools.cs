using System.Reflection;

namespace NetBackend.Tools;

public class DtoTools
{
    public static object GetDtoStructure(Type dtoType)
    {
        var properties = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(prop => new
            {
                Name = prop.Name,
                Type = GetFriendlyTypeName(prop.PropertyType)
            })
            .ToList();

        return new { Properties = properties };
    }

    public static string GetFriendlyTypeName(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return $"{GetFriendlyTypeName(type.GetGenericArguments()[0])}?";
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return $"List<{GetFriendlyTypeName(type.GetGenericArguments()[0])}>";
        }
        else
        {
            // Return a simplified type name for common types
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => "bool",
                TypeCode.Int32 => "int",
                TypeCode.String => "string",
                _ => type.Name,
            };
        }
    }
}
