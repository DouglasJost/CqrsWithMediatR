using CqrsWithMediatR.Models;
using MediatR;
using System.Collections.Generic;

namespace CqrsWithMediatR.Features.Queries
{
    public record GetProductsByPriceQuery(decimal Price, string Operator): IRequest<List<ProductReadOnly>>;
}
