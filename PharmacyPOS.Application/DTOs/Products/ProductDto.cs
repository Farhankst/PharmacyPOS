using System;
using System.Collections.Generic;
using System.Text;

namespace PharmacyPOS.Application.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }
    }
}
