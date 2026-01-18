using MassTransit;
using Microsoft.EntityFrameworkCore;
using Nexus.Shared.Messages;
using Nexus.Stock.API.Infra.Data;
using Nexus.Stock.API.Domain.Models; 

namespace Nexus.Stock.API.Application.Consumers;

public class StockUpdateConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly StockDbContext _context;
    private readonly ILogger<StockUpdateConsumer> _logger;

    public StockUpdateConsumer(StockDbContext context, ILogger<StockUpdateConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation($"[RabbitMQ] Iniciando processamento do Pedido: {context.Message.OrderId}");

        foreach (var item in context.Message.Items)
        {
            try
            {
                // Busca a variação diretamente pelo ID
                var variant = await _context.ProductVariants.FindAsync(item.VariantId);

                if (variant == null)
                {
                    _logger.LogError($"[ERRO GRAVE] Variação {item.VariantId} NÃO EXISTE no banco de dados! Verifique se você copiou o ID da VARIANT e não do PRODUCT.");
                    continue; 
                }

                _logger.LogInformation($"[SUCESSO] Produto encontrado: {variant.Size} - {variant.Color}. Estoque atual: {variant.Quantity}. Baixando: {item.Quantity}");

                // --- A CORREÇÃO ESTÁ AQUI ---
                // Mudamos de RemoveStock para DecreaseQuantity para bater com a Entidade e os Testes
                variant.DecreaseQuantity(item.Quantity); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ERRO DE CÓDIGO] Falha ao processar item {item.VariantId}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("[RabbitMQ] Processamento finalizado e salvo no banco.");
    }
}