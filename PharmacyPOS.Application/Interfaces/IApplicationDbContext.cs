using Microsoft.EntityFrameworkCore;
using PharmacyPOS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmacyPOS.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default
        );
    }
}
