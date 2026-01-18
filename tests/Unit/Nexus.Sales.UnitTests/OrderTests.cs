using Xunit;
using Nexus.Sales.API.Domain.Models; 
using System;

namespace Nexus.Sales.UnitTests
{
    public class OrderTests
    {
        [Fact(DisplayName = "Deve calcular o valor total do pedido corretamente")]
        public void Should_Calculate_Total_Amount_Correctly()
        {
            // 1. Arrange
            // Criamos um pedido para o cliente "Cliente123"
            var order = new Order("Cliente123");
            
            // 2. Act
            // Adiciona Sapatilha: R$ 150,00 x 2 = R$ 300,00
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Sapatilha", 150.00m, 2);
            
            // Adiciona Meia: R$ 20,00 x 3 = R$ 60,00
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Meia", 20.00m, 3);

            // 3. Assert
            // Total esperado: 300 + 60 = 360
            Assert.Equal(360.00m, order.TotalAmount);
        }

        [Fact(DisplayName = "Deve iniciar o pedido com status Pendente")]
        public void Should_Initialize_Order_With_Pending_Status()
        {
            // 1. Arrange & Act
            var order = new Order("ClienteXYZ");

            // 2. Assert
            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Equal(0, order.TotalAmount);
            Assert.Empty(order.Items);
        }

        [Fact(DisplayName = "Deve lançar erro ao adicionar item com quantidade zero")]
        public void Should_Throw_Error_When_Quantity_Is_Invalid()
        {
            // 1. Arrange
            var order = new Order("ClienteErro");

            // 2. Act & Assert
            // Tenta adicionar item com quantidade 0
            Assert.Throws<InvalidOperationException>(() => 
                order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Produto Ruim", 50.00m, 0)
            );
        }
    }
}