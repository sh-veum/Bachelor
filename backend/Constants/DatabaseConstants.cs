using NetBackend.Models;
using NetBackend.Models.Dto;

namespace NetBackend.Constants;

public static class DatabaseConstants
{
    public const string MainDbName = "Main";
    public const string CustomerOneDbName = "Customer1";
    public const string CustomerTwoDbName = "Customer2";

    public static readonly List<ApiEndpointDto> DefaultApiEndpoints =
    [
        new() {
            Path = "/aqua-culture-lists/fishhealth/licenseelist",
            Method = "POST",
            ExpectedBodyType = typeof(OrganizationDto)
        },
        new() {
            Path = "/aqua-culture-lists/fishhealth/species",
            Method = "POST",
            ExpectedBodyType = typeof(Species)
        }
    ];
}