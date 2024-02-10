using Microsoft.AspNetCore.Identity;
using NetBackend.Constants;
using NetBackend.Models.User;

public static class ApplicationDbInitializer
{
    public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(RoleConstants.AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleConstants.AdminRole));
        }

        if (!await roleManager.RoleExistsAsync(RoleConstants.CustomerRole))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleConstants.CustomerRole));
        }
    }

    public static async Task EnsureUser(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, string userEmail, string userPassword, string roleName)
    {
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            user = new User
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, userPassword);
        }

        if (user.DatabaseName == null)
        {
            user.DatabaseName = "Customer1";
            await userManager.UpdateAsync(user);
        }

        // Ensure the role exists
        await SeedRoles(roleManager);

        // Assign the role to the user if not already assigned
        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            await userManager.AddToRoleAsync(user, roleName);
        }
    }
}