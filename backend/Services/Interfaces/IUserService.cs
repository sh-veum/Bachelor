using Microsoft.AspNetCore.Mvc;
using NetBackend.Models.User;

namespace NetBackend.Services.Interfaces;

public interface IUserService
{
    Task<(UserModel user, ActionResult error)> GetUserAsync(HttpContext httpContext);
}