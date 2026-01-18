namespace Nexus.Sales.API.Domain.Models;

public enum OrderStatus { Pending, Paid, Cancelled }

public class Order
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CustomerId { get; private set; } = null!; 
    
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    protected Order() { }

    public Order(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("O ID do cliente é obrigatório.");

        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        TotalAmount = 0; // Começa zerado
    }

    public void AddItem(Guid productId, Guid variantId, string productName, decimal unitPrice, int quantity)
    {
        // Ao instanciar o OrderItem, as validações de qtd/preço (que criamos acima) serão chamadas
        var item = new OrderItem(Id, productId, variantId, productName, unitPrice, quantity);
        
        _items.Add(item);
        
        // Atualiza o total do pedido somando o total deste novo item
        TotalAmount += item.Total;
    }

    public void MarkAsPaid() => Status = OrderStatus.Paid;
    public void Cancel() => Status = OrderStatus.Cancelled;
}