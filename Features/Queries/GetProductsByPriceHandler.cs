using CqrsWithMediatR.Data;
using CqrsWithMediatR.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Features.Queries
{
    public class GetProductsByPriceHandler: IRequestHandler<GetProductsByPriceQuery, List<ProductReadOnly>>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public GetProductsByPriceHandler(IDbContextFactory<ApplicationDbContext> dbContextFactory) 
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<ProductReadOnly>> Handle(GetProductsByPriceQuery request, CancellationToken cancellationToken) 
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                IQueryable<ProductReadOnly> query = dbContext.ProductsReadOnly;
                switch (request.Operator)
                {
                    case "==":
                    {
                        query = query.Where(p => p.Price == request.Price);
                        break;
                    }
                    case "<":
                    {
                        query = query.Where(p => p.Price < request.Price);
                        break;
                    }
                    case "<=":
                    {
                        query = query.Where(p => p.Price <= request.Price);
                        break;
                    }
                    case ">":
                    {
                        query = query.Where(p => p.Price > request.Price);
                        break;
                    }
                    case ">=":
                    {
                        query = query.Where(p => p.Price >= request.Price);
                        break;
                    }
                    default:
                    {
                        throw new ArgumentException("Invalid comparaison operator.  Allowed values: ==, <, <=, >, >=.");
                    }
                }

                var foundProducts = new List<ProductReadOnly>();

                var products = await query.ToListAsync(cancellationToken);
                if (products == null)
                {
                    return foundProducts;
                }

                foreach(var product in products)
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
