using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Models.User;
using NetBackend.Services.Interfaces;

namespace NetBackend.Services;

public class UserService : IUserService
{
    private readonly UserManager<UserModel> _userManager;

    public UserService(UserManager<UserModel> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(UserModel user, ActionResult error)> GetUserAsync(HttpContext httpContext)
    {
        var user = await _userManager.GetUserAsync(httpContext.User);
        if (user == null)
        {
            var httpContextUser = httpContext.User.Identity?.Name;

            return (null!, new UnauthorizedObjectResult(new
            {
                message = $"User not found. {httpContextUser}"
            }));
        }

        return (user, null!);
    }
}