using CqrsWithMediatR.Models;
using MediatR;

namespace CqrsWithMediatR.Features.Queries
{
    public record GetProductByIdQuery(int Id) : IRequest<ProductReadOnly>;
}
