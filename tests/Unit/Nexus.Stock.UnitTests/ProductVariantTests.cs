using Xunit;
using Nexus.Stock.API.Domain.Models; // Namespace corrigido para Models
using System;

namespace Nexus.Stock.UnitTests
{
    public class ProductVariantTests
    {
        [Fact(DisplayName = "Deve diminuir o estoque corretamente quando houver saldo")]
        public void Should_Decrease_Stock_Successfully()
        {
            // 1. Arrange
            // O construtor exige (ProductId, Size, Color, Quantity)
            // Passamos um Guid aleatório pois não importa para este teste
            var variant = new ProductVariant(Guid.NewGuid(), "36", "Rosa", 20);

            // 2. Act
            variant.DecreaseQuantity(5);

            // 3. Assert
            Assert.Equal(15, variant.Quantity);
        }

        [Fact(DisplayName = "Deve lançar erro ao tentar vender mais do que tem no estoque")]
        public void Should_Throw_Exception_When_Stock_Is_Insufficient()
        {
            // 1. Arrange
            var variant = new ProductVariant(Guid.NewGuid(), "38", "Preta", 5);

            // 2. Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => variant.DecreaseQuantity(10));
            
            // Verifica se a mensagem de erro contém o texto esperado
            Assert.Contains("Estoque insuficiente", exception.Message);
        }

        [Fact(DisplayName = "Deve lançar erro ao tentar reduzir valor negativo ou zero")]
        public void Should_Throw_Exception_When_Amount_Is_Invalid()
        {
            // 1. Arrange
            var variant = new ProductVariant(Guid.NewGuid(), "40", "Branca", 10);

            // 2. Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => variant.DecreaseQuantity(0));
            
            Assert.Equal("A quantidade a ser reduzida deve ser maior que zero.", exception.Message);
        }
    }
}