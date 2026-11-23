using Microsoft.EntityFrameworkCore;
using PharmacyPOS.Application.Interfaces;
using PharmacyPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmacyPOS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
    }
}
