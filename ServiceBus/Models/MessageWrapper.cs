using System.Text.Json;

namespace CqrsWithMediatR.ServiceBus.Models
{
    public class MessageWrapper
    {
        public required string EventType { get; set; }
        public JsonElement Payload { get; set; }
    }
}
