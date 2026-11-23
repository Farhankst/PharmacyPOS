using Microsoft.EntityFrameworkCore;
using PharmacyPOS.Application.Interfaces;
using PharmacyPOS.Application.Services;
using PharmacyPOS.Infrastructure.Data;
using PharmacyPOS.Web.Components;
using PharmacyPOS.Web.Services.ApiClients;
using PharmacyPOS.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddScoped<IApplicationDbContext>(
    provider => provider.GetRequiredService<ApplicationDbContext>()
);

// Application Services
builder.Services.AddScoped<IProductService, ProductService>();

// API Client
builder.Services.AddHttpClient<ProductApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// API Endpoints
app.MapProductEndpoints();

// Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();