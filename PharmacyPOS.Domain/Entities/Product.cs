using System;
using System.Collections.Generic;
using System.Text;

namespace PharmacyPOS.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public bool Active { get; set; } = true;

        public bool Archive { get; set; } = false;
    }
}
