using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.Stock.API.Application.DTOs;
using Nexus.Stock.API.Domain.Models;
using Nexus.Stock.API.Infra.Data;

namespace Nexus.Stock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // <--- 2. A FECHADURA: Bloqueia a classe inteira. Sem token, sem acesso.
public class ProductsController : ControllerBase
{
    private readonly StockDbContext _context;

    public ProductsController(StockDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Cadastra um novo produto com suas variações (Grade).
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/products
    ///     {
    ///        "name": "Sapatilha",
    ///        "basePrice": 100,
    ///        "variants": [{ "size": "37", "color": "Rosa", "quantity": 10 }]
    ///     }
    /// </remarks>
    /// <param name="request">Dados do produto e grade.</param>
    /// <returns>O produto criado com ID.</returns>
    /// <response code="201">Produto cadastrado com sucesso.</response>
    /// <response code="400">Dados inválidos (ex: campos obrigatórios faltando).</response>
    /// <response code="401">Não autorizado (Faltou o Token).</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documentamos que pode dar erro de auth
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        // Validação básica manual (opcional, pois o [ApiController] já valida o DTO)
        if (!ModelState.IsValid)
             return BadRequest(ModelState);

        var product = new Product(request.Name, request.Description, request.BasePrice);

        foreach (var v in request.Variants)
        {
            try 
            {
                product.AddVariant(v.Size, v.Color, v.Quantity);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro na variação: {ex.Message}");
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Busca um produto pelo ID.
    /// </summary>
    /// <param name="id">ID do produto (Guid).</param>
    /// <returns>Detalhes do produto e suas variações.</returns>
    /// <response code="200">Produto encontrado.</response>
    /// <response code="404">Produto não encontrado.</response>
    /// <response code="401">Não autorizado.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        return Ok(product);
    }
}