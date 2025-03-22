using Azure.Identity;
using Azure.Messaging.ServiceBus;
using CqrsWithMediatR.ServiceBus.Models;
using CqrsWithMediatR.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CqrsWithMediatR.ServiceBus.Services
{
    //Implement ServiceBusPublisher to Send Messages
    public class ServiceBusPublisher
    {
        private readonly IKeyVaultService _keyVaultService;
        private readonly string _queueName;
        private readonly string _namespace;

        public ServiceBusPublisher(IKeyVaultService keyVaultService)
        {
            _keyVaultService = keyVaultService;
            _queueName = _keyVaultService.GetKeyValue(KeyVaultService.ServiceBusQueueName);
            _namespace = _keyVaultService.GetKeyValue(KeyVaultService.ServiceBusNamespace);
        }

        public async Task SendMessageAsync<T>(T eventMessage) where T : class
        {
            await using ServiceBusClient client = new ServiceBusClient(_namespace, new DefaultAzureCredential());
            ServiceBusSender sender = client.CreateSender(_queueName);

            // Convert eventMessage to JSON string
            string jsonPayload = JsonSerializer.Serialize(eventMessage);

            // Deserialize JSON string back to JsonElement
            JsonElement payloadElement = JsonSerializer.Deserialize<JsonElement>(jsonPayload);

            var wrappedMessage = new MessageWrapper()
            {
                EventType = typeof(T).Name,
                Payload = payloadElement
            };

            string messageBody = JsonSerializer.Serialize(wrappedMessage);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
            {
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
            Console.WriteLine($"Sent ProductCreatedEvent: {messageBody}");
        }
    }
}
