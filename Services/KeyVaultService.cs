using System;

namespace CqrsWithMediatR.Services
{
    public static class KeyVaultService
    {
        public static readonly string ServiceBusNamespace = "ServiceBusNamespace";
        public static readonly string ServiceBusQueueName = "ServiceBusQueueName";

        public static string GetKeyValue(string keyValue)
        {
            var value = Environment.GetEnvironmentVariable(keyValue)
                ?? throw new InvalidOperationException($"The environment variable '{keyValue}' is not set in the Development environment.");

            return value;
        }
    }
}
