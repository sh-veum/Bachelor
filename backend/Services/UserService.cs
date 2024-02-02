using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Models.User;

namespace Netbackend.Services;

public interface IUserService
{
    Task<(User user, ActionResult error)> GetUserAsync(HttpContext httpContext);
}


public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(User user, ActionResult error)> GetUserAsync(HttpContext httpContext)
    {
        var user = await _userManager.GetUserAsync(httpContext.User);
        if (user == null)
        {
            return (null!, new UnauthorizedResult());
        }

        return (user, null!);
    }
}