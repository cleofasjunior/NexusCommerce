using System.Text.Json.Serialization;

namespace Nexus.Stock.API.Domain.Models; // Note o namespace correto: Models

public class Product
{
    public Guid Id { get; private set; }
    
    public string Name { get; private set; } = null!; 
    public string Description { get; private set; } = null!;
    public decimal BasePrice { get; private set; }
    
    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    protected Product() { }

    public Product(string name, string description, decimal basePrice)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        BasePrice = basePrice;
    }

    public void AddVariant(string size, string color, int quantity)
    {
        if (_variants.Any(v => v.Size == size && v.Color == color))
            throw new InvalidOperationException($"A variação {size} - {color} já existe neste produto.");

        _variants.Add(new ProductVariant(Id, size, color, quantity));
    }
}

public class ProductVariant
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    
    public string Size { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    
    public int Quantity { get; private set; }

    public byte[] RowVersion { get; set; } = null!;

    protected ProductVariant() { }

    // Construtor que o EF Core e nós usamos
    public ProductVariant(Guid productId, string size, string color, int quantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Size = size;
        Color = color;
        Quantity = quantity;
    }

    // --- MÉTODO AJUSTADO PARA O TESTE ---
    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("A quantidade a ser reduzida deve ser maior que zero.");

        if (Quantity < amount)
            throw new InvalidOperationException($"Estoque insuficiente para {Size}/{Color}. Disponível: {Quantity}, Tentativa: {amount}");
        
        Quantity -= amount;
    }
    
    public void AddStock(int amount)
    {
        if (amount <= 0) 
            throw new InvalidOperationException("A quantidade a adicionar deve ser positiva.");

        Quantity += amount;
    }
}