using System;
using System.Text.Json.Serialization;

namespace CqrsWithMediatR.ServiceBus.Events
{
    public class ProductCreatedEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore]
        public byte[] RowVersion { get; set; }

        [JsonPropertyName("RowVersion64")]
        public string RowVersionBase64 
        {
            get => Convert.ToBase64String(RowVersion);
            set => RowVersion = Convert.FromBase64String(value);
        }

        public ProductCreatedEvent(int id, string name, decimal price, byte[] rowVersion)
        {
            Id = id;
            Name = name;
            Price = price;
            RowVersion = rowVersion;
        }
    }
}
