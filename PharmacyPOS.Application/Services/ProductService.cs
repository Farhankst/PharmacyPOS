using Microsoft.EntityFrameworkCore;
using PharmacyPOS.Application.DTOs.Products;
using PharmacyPOS.Application.Interfaces;
using PharmacyPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmacyPOS.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IApplicationDbContext _db;

        public ProductService(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            return await _db.Products
                .AsNoTracking()
                .Where(x => !x.Archive && x.Active)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    UnitPrice = x.UnitPrice
                })
                .ToListAsync();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            return await _db.Products
                .AsNoTracking()
                .Where(x => x.Id == id && !x.Archive)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    UnitPrice = x.UnitPrice
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductDto?> UpdateAsync(int id, CreateProductDto model)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(x => x.Id == id && !x.Archive);

            if (product == null)
                return null;

            var code = model.Code?.Trim() ?? string.Empty;
            var name = model.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product code and name are required.");

            if (model.UnitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative.");

            var normalizedCode = code.ToUpperInvariant();
            if (await _db.Products.AnyAsync(x => x.Id != id &&
                !x.Archive && x.Code.Trim().ToUpper() == normalizedCode))
                throw new InvalidOperationException("Product code already exists.");

            product.Code = code;
            product.Name = name;
            product.UnitPrice = model.UnitPrice;

            await _db.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                UnitPrice = product.UnitPrice
            };
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto model)
        {
            var product = new Product
            {
                Code = model.Code,
                Name = model.Name,
                UnitPrice = model.UnitPrice,
                Active = true,
                Archive = false
            };

            _db.Products.Add(product);

            await _db.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                UnitPrice = product.UnitPrice
            };
        }
    }
}
