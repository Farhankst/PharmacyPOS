using Microsoft.AspNetCore.Identity;
using PharmacyPOS.Web.Data;

namespace PharmacyPOS.Web.Security;

public static class IdentityBootstrap
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        await using var transaction = await db.Database.BeginTransactionAsync();
        foreach (var role in AppRoles.All)
        {
            if (!await roles.RoleExistsAsync(role))
                Ensure(await roles.CreateAsync(new IdentityRole(role)));
        }

        if ((await users.GetUsersInRoleAsync(AppRoles.Admin)).Count == 0)
        {
            var username = (configuration["IdentityBootstrap:Username"] ?? configuration["IdentityBootstrap:Email"])?.Trim();
            var password = configuration["IdentityBootstrap:Password"];
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Configure the initial admin username and password through secrets.");

            if (await users.FindByNameAsync(username) != null)
                throw new InvalidOperationException("Bootstrap will not promote an existing account.");

            var admin = new ApplicationUser { UserName = username, LockoutEnabled = true };
            Ensure(await users.CreateAsync(admin, password));
            Ensure(await users.AddToRoleAsync(admin, AppRoles.Admin));
        }
        await transaction.CommitAsync();
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
