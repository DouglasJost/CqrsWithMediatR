using CqrsWithMediatR.Data;
using CqrsWithMediatR.Models;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using CqrsWithMediatR.ServiceBus.Services;
using CqrsWithMediatR.ServiceBus.Events;
using Microsoft.EntityFrameworkCore;

namespace CqrsWithMediatR.Features.Commands
{
    //Modify CreateProductHandler to Publish Events
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly ServiceBusPublisher _serviceBusPublisher;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public CreateProductHandler(
            ServiceBusPublisher serviceBusPublisher,
            IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _serviceBusPublisher = serviceBusPublisher;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var product = new Product 
                { 
                    Name = request.Name, 
                    Price = request.Price
                };

                // For In-Memory database only, need to assign an initial value for RowVersion
                product.RowVersion = new byte[8];

                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync(cancellationToken);

                // Publish event to Azure Service Bus
                var productEvent = new ProductCreatedEvent(product.Id, product.Name, product.Price, product.RowVersion);

                // Fire-and-forget event publishing (does not block response)
                _ = Task.Run(async () =>
                {
                    await _serviceBusPublisher.SendMessageAsync(productEvent);
                });

                return product.Id;
            }
        }
    }
}
