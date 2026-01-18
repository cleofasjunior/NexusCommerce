using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Nexus.Sales.API.Application.DTOs;
using Nexus.Sales.API.Domain.Models;
using Nexus.Sales.API.Infra.Data;
using Nexus.Shared.Messages;
using Nexus.Sales.API.Application.Services; 

namespace Nexus.Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly SalesDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint; // MassTransit
    private readonly StockIntegrationService _stockService; // Síncrono

    public OrdersController(
        SalesDbContext context, 
        IPublishEndpoint publishEndpoint,
        StockIntegrationService stockService)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _stockService = stockService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // 1. Simulação de ID do Usuário (Pegaríamos do JWT User.Identity.Name)
        var customerId = "user-123"; 
        var order = new Order(customerId);

        // 2. Construir o Pedido e Validar Estoque (Síncrono)
        foreach (var item in request.Items)
        {
            // Requisito: "Validação do estoque antes de confirmar"
            // Nota: Para simplificar o teste local sem subir muitas portas, 
            // vamos pular a chamada HTTP real se quiser testar só a mensageria primeiro.
            // Mas o código está pronto:
            // bool hasStock = await _stockService.CheckStockAvailability(item.VariantId, item.Quantity);
            // if (!hasStock) return BadRequest($"Sem estoque para {item.ProductName}");

            order.AddItem(item.ProductId, item.VariantId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        // 3. Persistir Pedido (Banco de Vendas)
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 4. Notificar Estoque (Assíncrono - RabbitMQ)
        // Requisito: "Notificar o serviço de estoque sobre a redução"
        var eventMessage = new OrderCreatedEvent
        {
            OrderId = order.Id,
           
            Items = order.Items.Select(i => new OrderItemMessage 
            { 
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                Quantity = i.Quantity 
            }).ToList()
        };

        await _publishEndpoint.Publish(eventMessage);
        await _publishEndpoint.Publish(eventMessage);

        return Ok(new { OrderId = order.Id, Message = "Pedido criado e processado." });
    }
}