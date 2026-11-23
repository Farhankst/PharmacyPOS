using Microsoft.AspNetCore.Components;
using PharmacyPOS.Application.DTOs.Products;

namespace PharmacyPOS.Web.Services.ApiClients
{
    public class ProductApiClient
    {
        private readonly HttpClient _httpClient;

        public ProductApiClient(
            HttpClient httpClient,
            NavigationManager navigation)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress =
                new Uri(navigation.BaseUri);
        }

        // Get All
        public async Task<List<ProductDto>> GetAllAsync()
        {
            var result = await _httpClient
                .GetFromJsonAsync<List<ProductDto>>(
                    "api/products");

            return result ?? new List<ProductDto>();
        }

        // Get By Id
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            return await _httpClient
                .GetFromJsonAsync<ProductDto>(
                    $"api/products/{id}");
        }

        // Create
        public async Task<ProductDto?> CreateAsync(
            CreateProductDto model)
        {
            var response = await _httpClient
                .PostAsJsonAsync("api/products", model);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<ProductDto>();
        }
    }
}
