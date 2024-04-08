using NetBackend.Models.Dto;
using NetBackend.Models.Dto.Keys;

namespace NetBackend.Constants;

public static class ApiConstants
{
    public static readonly List<RestApiEndpointDto> DefaultApiEndpoints =
    [
        new()
        {
            Path = "/api/aquaculturelist/fishhealth/licenseelist",
            Method = "POST",
            ExpectedBodyType = typeof(OrganizationDto)
        },
        new()
        {
            Path = "/api/aquaculturelist/fishhealth/species",
            Method = "POST",
            ExpectedBodyType = typeof(SpeciesDto)
        },
        // Placeholder to show more themes:
        new()
        {
            Path = "/api/rest/accesskey-themes",
            Method = "POST",
            ExpectedBodyType = typeof(ThemeDto)
        },
        new()
        {
            Path = "/api/rest/accesskey-rest-endpoints",
            Method = "POST",
            ExpectedBodyType = typeof(RestApiEndpointSchema)
        }
    ];
}