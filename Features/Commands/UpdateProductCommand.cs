using MediatR;

namespace CqrsWithMediatR.Features.Commands
{
    public class UpdateProductCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public byte[] RowVersion { get; set; } = default!;
    }
}
