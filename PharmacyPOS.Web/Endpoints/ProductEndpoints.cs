using PharmacyPOS.Application.DTOs.Products;
using PharmacyPOS.Application.Interfaces;

namespace PharmacyPOS.Web.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        // Get All Products
        group.MapGet("/", async (IProductService service) =>
        {
            var products = await service.GetAllAsync();

            return Results.Ok(products);
        });

        // Get Product By Id
        group.MapGet("/{id:int}", async (
            int id,
            IProductService service) =>
        {
            var product = await service.GetByIdAsync(id);

            if (product == null)
                return Results.NotFound();

            return Results.Ok(product);
        });

        // Update Product
        group.MapPut("/{id:int}", async (
            int id,
            CreateProductDto model,
            IProductService service) =>
        {
            try
            {
                var product = await service.UpdateAsync(id, model);
                return product == null ? Results.NotFound() : Results.Ok(product);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }
        });

        // Create Product
        group.MapPost("/", async (
            CreateProductDto model,
            IProductService service) =>
        {
            var product = await service.CreateAsync(model);

            return Results.Created(
                $"/api/products/{product.Id}",
                product
            );
        });
    }
}