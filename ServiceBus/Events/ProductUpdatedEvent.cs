namespace CqrsWithMediatR.ServiceBus.Events
{
    public class ProductUpdatedEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public decimal Price { get; set; }
        public byte[] RowVersion { get; set; } 

        public ProductUpdatedEvent(int id, string name, decimal price, byte[] rowVersion)
        {
            Id = id;
            Name = name;
            Price = price;
            RowVersion = rowVersion;
        }
    }
}
