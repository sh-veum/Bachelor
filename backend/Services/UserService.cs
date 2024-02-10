using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetBackend.Models.User;

namespace NetBackend.Services;

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
            var httpContextUser = httpContext.User.Identity?.Name;

            return (null!, new UnauthorizedObjectResult(new
            {
                message = $"User not found. {httpContextUser}"
            }));
        }

        return (user, null!);
    }
}