using PharmacyPOS.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmacyPOS.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();

        Task<ProductDto?> GetByIdAsync(int id);

        Task<ProductDto?> UpdateAsync(int id, CreateProductDto model);

        Task<ProductDto> CreateAsync(CreateProductDto model);
    }
}
