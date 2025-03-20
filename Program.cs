
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using CqrsWithMediatR.Data;
using CqrsWithMediatR.ServiceBus.Configuration;
using CqrsWithMediatR.ServiceBus.Services;
using CqrsWithMediatR.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //
            // AddTransient - Create a new instance every time it is requested
            // AddScoped    - One instance per HTTP request
            // AddSingleton - One instance for the entire app
            //

            var builder = WebApplication.CreateBuilder(args);

            // Bind Azure Service Bus settings
            //builder.Services.Configure<AzureServiceBusSettings>(options => 
            //{
            //    options.QueueName = KeyVaultService.GetKeyValue(KeyVaultService.QueueName);
            //    options.Namespace = KeyVaultService.GetKeyValue(KeyVaultService.Namespace);
            //});

            // Add DB context as a Factory.
            //   When a db context is needed, the respective class will need to inject a 
            //   IDbContextFactory and retrieve a context similar to this:
            //
            //     await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
            //
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("ProductDB")
                        .LogTo(Console.WriteLine, LogLevel.Information));

            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseInMemoryDatabase("ProductDB")
            //           .LogTo(Console.WriteLine, LogLevel.Information));

            // Add MediatR : Scan all handlers in the same assembly as Program.cs
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // Register ServiceBusPublisher
            builder.Services.AddSingleton<ServiceBusPublisher>();

            // Register ServiceBusClient as Singleton (to prevent connection issues)
            builder.Services.AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var fullyQualifiedNamespace = KeyVaultService.GetKeyValue(KeyVaultService.ServiceBusNamespace);
                return new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential());
            });

            // Register ServiceBusConsumer
            builder.Services.AddSingleton<ServiceBusConsumer>();

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();  // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            var app = builder.Build();

            // Start the ServiceBusConsumer process
            var serviceBusConsumer = app.Services.GetRequiredService<ServiceBusConsumer>();
            await serviceBusConsumer.StartListening();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
