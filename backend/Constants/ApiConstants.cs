using NetBackend.Models;
using NetBackend.Models.Dto;
using NetBackend.Models.Dto.Keys;
using NetBackend.Models.Keys;

namespace NetBackend.Constants;

public static class ApiConstants
{
    public static readonly List<RestApiEndpointDto> DefaultApiEndpoints =
    [
        new() {
            Path = "/api/aquaculturelist/fishhealth/licenseelist",
            Method = "POST",
            ExpectedBodyType = typeof(OrganizationDto)
        },
        new() {
            Path = "/api/aquaculturelist/fishhealth/species",
            Method = "POST",
            ExpectedBodyType = typeof(SpeciesDto)
        },
        // Placeholder to show more themes:
        new() {
            Path = "/api/key/accesskey-themes",
            Method = "POST",
            ExpectedBodyType = typeof(ThemeDto)
        },
        new() {
            Path = "/api/key/accesskey-rest-endpoints",
            Method = "POST",
            ExpectedBodyType = typeof(RestApiEndpointSchema)
        }
    ];
}