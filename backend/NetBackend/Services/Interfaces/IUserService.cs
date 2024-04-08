using Microsoft.AspNetCore.Mvc;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces;

public interface IUserService
{
    Task<(UserModel user, ActionResult error)> GetUserByHttpContextAsync(HttpContext httpContext);
    Task<(UserModel user, ActionResult error)> GetUserByIdAsync(string userId);
}