using System;

namespace CqrsWithMediatR.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        public static readonly string ServiceBusNamespace = "ServiceBusNamespace";
        public static readonly string ServiceBusQueueName = "ServiceBusQueueName";

        public KeyVaultService() { }

        public string GetKeyValue(string keyValue)
        {
            var value = Environment.GetEnvironmentVariable(keyValue)
                ?? throw new InvalidOperationException($"The environment variable '{keyValue}' is not set in the Development environment.");

            return value;
        }
    }
}
