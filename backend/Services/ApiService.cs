using System.Reflection;

namespace NetBackend.Services;

public interface IApiService
{
    object GetDtoStructure(Type dtoType);
    string GetFriendlyTypeName(Type type);
}

public class ApiService : IApiService
{
    private readonly ILogger<KeyService> _logger;

    public ApiService(
        ILogger<KeyService> logger)
    {
        _logger = logger;
    }

    public object GetDtoStructure(Type dtoType)
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

    public string GetFriendlyTypeName(Type type)
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
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return "bool";
                case TypeCode.Int32: return "int";
                case TypeCode.String: return "string";
                default: return type.Name;
            }
        }
    }
}
