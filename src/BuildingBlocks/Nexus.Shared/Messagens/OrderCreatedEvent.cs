namespace Nexus.Shared.Messages;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public List<OrderItemMessage> Items { get; set; } = new();
}

public class OrderItemMessage
{
    public Guid ProductId { get; set; } 
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}