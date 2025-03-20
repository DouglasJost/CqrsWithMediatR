using CqrsWithMediatR.Models;
using MediatR;
using System.Collections.Generic;

namespace CqrsWithMediatR.Features.Queries
{
    public record GetAllProductsQuery() : IRequest<List<ProductReadOnly>>;
}
