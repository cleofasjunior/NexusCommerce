namespace Nexus.Sales.API.Domain.Models;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; } // Vital para a Loja Allegro (Saber se é 36 ou 37)
    
    public string ProductName { get; private set; } = null!;
    
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    
    // Propriedade calculada: Preço x Quantidade
    public decimal Total => UnitPrice * Quantity;

    protected OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, Guid variantId, string productName, decimal unitPrice, int quantity)
    {
        // --- VALIDAÇÕES NECESSÁRIAS PARA OS TESTES ---
        if (quantity <= 0) 
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");
            
        if (unitPrice < 0) 
            throw new InvalidOperationException("O preço unitário não pode ser negativo.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        VariantId = variantId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}