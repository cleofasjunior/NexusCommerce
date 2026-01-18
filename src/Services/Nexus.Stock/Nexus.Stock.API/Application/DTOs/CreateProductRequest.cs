using System.ComponentModel.DataAnnotations;

namespace Nexus.Stock.API.Application.DTOs;

public class CreateProductRequest
{
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    public string Name { get; set; } = string.Empty; // Inicializa vazio para evitar null

    public string Description { get; set; } = string.Empty; // Inicializa vazio

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal BasePrice { get; set; }

    [Required]
    // Inicializamos a lista para evitar erro se tentarem dar um .Add() ou count() nela sem instanciar
    public List<ProductVariantDto> Variants { get; set; } = new(); 
}

public class ProductVariantDto
{
    [Required(ErrorMessage = "O tamanho é obrigatório.")]
    public string Size { get; set; } = string.Empty;  // Ex: "37"

    [Required(ErrorMessage = "A cor é obrigatória.")]
    public string Color { get; set; } = string.Empty; // Ex: "Rosa"

    [Range(1, 9999, ErrorMessage = "A quantidade deve ser no mínimo 1.")]
    public int Quantity { get; set; }
}