using System.ComponentModel.DataAnnotations;

namespace Nexus.Sales.API.Application.DTOs;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "A lista de itens do pedido é obrigatória.")]
    
    public List<OrderItemRequest> Items { get; set; } = new(); 
}

public class OrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid VariantId { get; set; } // O ID específico do Tamanho/Cor

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    
    public string ProductName { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
    public int Quantity { get; set; }
}