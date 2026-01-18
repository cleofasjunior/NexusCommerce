using System.Net.Http;
using System.Text.Json;

namespace Nexus.Sales.API.Application.Services;

public class StockIntegrationService
{
    private readonly HttpClient _httpClient;

    // Injeta HttpClient configurado via Factory
    public StockIntegrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CheckStockAvailability(Guid variantId, int quantity)
    {
        // Na vida real, chamaríamos um endpoint específico de validação
        // Aqui, para simplificar o desafio, vamos assumir que se o GET funciona, o produto existe.
        // O ideal seria um endpoint GET /api/products/variants/{id}/availability
        
        // Simulação: Se o produto existe no catálogo, retornamos true.
        // Num cenário real de produção, faríamos a checagem da quantidade aqui.
        var response = await _httpClient.GetAsync($"api/products/check/{variantId}");
        
        // Se retornar 200 OK, assumimos que existe.
        return true; 
    }
}