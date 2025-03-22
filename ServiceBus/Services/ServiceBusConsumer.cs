using Azure.Messaging.ServiceBus;
using CqrsWithMediatR.Data;
using CqrsWithMediatR.Models;
using CqrsWithMediatR.ServiceBus.Events;
using CqrsWithMediatR.ServiceBus.Models;
using CqrsWithMediatR.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CqrsWithMediatR.ServiceBus.Services
{
    //Create ServiceBusConsumer.cs to Listen for Messages:
    public class ServiceBusConsumer
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IKeyVaultService _keyVaultService;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _queueName;

        public ServiceBusConsumer(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            IKeyVaultService keyVaultService,
            ServiceBusClient serviceBusClient)
        {
            _dbContextFactory = dbContextFactory;
            _keyVaultService = keyVaultService;
            _serviceBusClient = serviceBusClient;
            _queueName = _keyVaultService.GetKeyValue(KeyVaultService.ServiceBusQueueName);
        }

        public async Task StartListening()
        {
            ServiceBusProcessor processor = _serviceBusClient.CreateProcessor(_queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,   // Ensures messages are completed only after processing
                MaxConcurrentCalls = 5,         // Processes multiple messages in parallel
                PrefetchCount = 10,             // Fetches messages in batches
            });

            processor.ProcessMessageAsync += MessageHandlerAsync;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();
        }

        private async Task MessageHandlerAsync(ProcessMessageEventArgs args)
        {
            string body = Encoding.UTF8.GetString(args.Message.Body);
            var wrapper = JsonSerializer.Deserialize<MessageWrapper>(body);

            if (wrapper == null || wrapper.Payload.ValueKind == JsonValueKind.Undefined)
            {
                // throw an exception and-or log exception
                return;
            }

            if (wrapper.EventType == nameof(ProductCreatedEvent))
            {
                await HandleProductCreateEvent(wrapper);
            }
            else if (wrapper.EventType == nameof(ProductUpdatedEvent))
            {
                await HandleProductUpdatedEvent(wrapper);
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task HandleProductCreateEvent(MessageWrapper wrapper)
        {
            var productEvent = wrapper.Payload.Deserialize<ProductCreatedEvent>();
            if (productEvent == null)
            {
                throw new JsonException("Failed to deserialize ProductCreatedEvent from message.");
            }

            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var product = new ProductReadOnly()
                {
                    Id = productEvent.Id,
                    Name = productEvent.Name,
                    Price = productEvent.Price,
                    RowVersion = productEvent.RowVersion
                };

                dbContext.ProductsReadOnly.Add(product);
                await dbContext.SaveChangesAsync();

                Console.WriteLine($"ProductsReadOnly Created: PK:{productEvent.Id}, Name:{productEvent.Name}, Price:{productEvent.Price}, RowVersion: {productEvent.RowVersion}");
            }
        }

        private async Task HandleProductUpdatedEvent(MessageWrapper wrapper)
        {
            var productEvent = wrapper.Payload.Deserialize<ProductUpdatedEvent>();
            if (productEvent == null)
            {
                throw new JsonException("Failed to deserialize ProductUpdatedEvent from message.");
            }

            await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            {
                var existingProduct = await dbContext.ProductsReadOnly.FindAsync(productEvent.Id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"ServiceBusConsumer: Product {productEvent.Id} not found for update.");
                }

                existingProduct.Name = productEvent.Name;
                existingProduct.Price = productEvent.Price;
                existingProduct.RowVersion = productEvent.RowVersion;
                await dbContext.SaveChangesAsync();

                Console.WriteLine($"ProductsReadOnly Updated: PK:{productEvent.Id}, Name:{productEvent.Name}, Price:{productEvent.Price}, RowVersion: {productEvent.RowVersion}");
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Service Bus Error: {args.Exception.Message}");

            if (args.Exception is ServiceBusException sbEx)
            {
                Console.WriteLine($"Service Bus Error Code: {sbEx.Reason}");
            }

            Console.WriteLine($"Fully Qualified Namespace: {args.FullyQualifiedNamespace}");
            Console.WriteLine($"Entity Path: {args.EntityPath}");

            return Task.CompletedTask;
        }
    }
}
