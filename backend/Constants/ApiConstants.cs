using NetBackend.Models;
using NetBackend.Models.Dto;

namespace NetBackend.Constants;

public static class ApiConstants
{
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
            ExpectedBodyType = typeof(SpeciesDto)
        }
    ];
}