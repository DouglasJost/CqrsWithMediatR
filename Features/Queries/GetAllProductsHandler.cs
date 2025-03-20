using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CqrsWithMediatR.Data;
using CqrsWithMediatR.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System;

namespace CqrsWithMediatR.Features.Queries
{
    public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, List<ProductReadOnly>>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public GetAllProductsHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<ProductReadOnly>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var foundProducts = new List<ProductReadOnly>();
                var products = await dbContext.ProductsReadOnly.ToListAsync(cancellationToken);
                if (products == null)
                {
                    return foundProducts;
                }

                foreach (var product in products)
                {
                    var prod = new ProductReadOnly()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        RowVersionBase64 = Convert.ToBase64String(product.RowVersion)
                    };
                    foundProducts.Add(prod);
                }

                return foundProducts;
            }
        }
    }
}
