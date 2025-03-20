using MediatR;

namespace CqrsWithMediatR.Features.Commands
{
    public record CreateProductCommand(string Name, decimal Price) : IRequest<int>;
}
