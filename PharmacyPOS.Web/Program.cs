using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyPOS.Application.Interfaces;
using PharmacyPOS.Application.Services;
using PharmacyPOS.Web.Components;
using PharmacyPOS.Web.Components.Account;
using PharmacyPOS.Web.Data;
using PharmacyPOS.Web.Endpoints;
using PharmacyPOS.Web.Security;
using ProductDbContext = PharmacyPOS.Infrastructure.Data.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is required.");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ProductDbContext>());
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    options.User.RequireUniqueEmail = false; // Email is optional for username-based staff accounts.
    // Accounts are provisioned by an administrator; no public registration or email verification.
    options.SignIn.RequireConfirmedAccount = false;
    // Temporary simple passwords only for local development.
    var development = builder.Environment.IsDevelopment();
    options.Password.RequiredLength = development ? 6 : 12;
    options.Password.RequireDigit = !development;
    options.Password.RequireLowercase = !development;
    options.Password.RequireUppercase = !development;
    options.Password.RequireNonAlphanumeric = !development;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.Zero);
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AccessPolicies.ProductRead, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Manager, AppRoles.Cashier));
    options.AddPolicy(AccessPolicies.ProductWrite, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Manager));
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<AuthorizedProductService>();

var app = builder.Build();

// Explicit one-time provisioning; apply the Identity migration before enabling this.
if (app.Configuration.GetValue<bool>("IdentityBootstrap:Enabled"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await IdentityBootstrap.InitializeAsync(scope.ServiceProvider, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapProductEndpoints();
app.MapAdditionalIdentityEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
