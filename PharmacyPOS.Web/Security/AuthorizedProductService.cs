using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using PharmacyPOS.Application.DTOs.Products;
using PharmacyPOS.Application.Interfaces;
using PharmacyPOS.Web.Data;

namespace PharmacyPOS.Web.Security;

// Interactive Server calls stay server-side; do not forward cookies through a loopback HttpClient.
// Each operation gets a fresh scope and rechecks the live user, security stamp and role.
public sealed class AuthorizedProductService(
    AuthenticationStateProvider authenticationState, IServiceScopeFactory scopeFactory)
{
    private async Task<T> ExecuteAsync<T>(bool write, Func<IProductService, Task<T>> action)
    {
        var principal = (await authenticationState.GetAuthenticationStateAsync()).User;
        if (principal.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException();

        await using var scope = scopeFactory.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signIn = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var user = await signIn.ValidateSecurityStampAsync(principal);
        if (user is null || await users.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException();

        var roles = await users.GetRolesAsync(user);
        if (!(roles.Contains(AppRoles.Admin) || roles.Contains(AppRoles.Manager) ||
              (!write && roles.Contains(AppRoles.Cashier))))
            throw new UnauthorizedAccessException();

        return await action(scope.ServiceProvider.GetRequiredService<IProductService>());
    }

    public Task<List<ProductDto>> GetAllAsync() => ExecuteAsync(false, s => s.GetAllAsync());
    public Task<ProductDto?> GetByIdAsync(int id) => ExecuteAsync(false, s => s.GetByIdAsync(id));
    public Task<ProductDto> CreateAsync(CreateProductDto model) => ExecuteAsync(true, s => s.CreateAsync(model));
    public Task<ProductDto?> UpdateAsync(int id, CreateProductDto model) => ExecuteAsync(true, s => s.UpdateAsync(id, model));
}
