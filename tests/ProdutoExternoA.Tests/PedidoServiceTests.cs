using Moq;
using ProdutoExternoA.Domain;
using ProdutoExternoA.Infrastructure;
using ProdutoExternoA.Service;
using Microsoft.Extensions.Logging;

namespace ProdutoExternoA.Tests;

public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _repositoryMock;
    private readonly Mock<ICalculadoraImpostoFactory> _factoryMock;
    private readonly Mock<IRabbitMqPublisher> _publisherMock;
    private readonly Mock<ILogger<PedidoService>> _loggerMock;
    private readonly PedidoService _pedidoService;

    public PedidoServiceTests()
    {
        _repositoryMock = new Mock<IPedidoRepository>();
        _factoryMock = new Mock<ICalculadoraImpostoFactory>();
        _publisherMock = new Mock<IRabbitMqPublisher>();
        _loggerMock = new Mock<ILogger<PedidoService>>();

        _pedidoService = new PedidoService(
            _repositoryMock.Object,
            _factoryMock.Object,
            _publisherMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ProcessarPedidoAsync_DeveProcessarCorretamente()
    {
        // Arrange
        var request = new CriarPedidoRequest(
            PedidoId: 1,
            ClienteId: 10,
            Itens: new List<PedidoItem>
            {
                new PedidoItem(ProdutoId: 100, Quantidade: 2, Valor: 50),
                new PedidoItem(ProdutoId: 101, Quantidade: 1, Valor: 100)
            }
        );

        var impostoMock = new Mock<IImposto>();
        impostoMock.Setup(i => i.Calcular(It.IsAny<decimal>())).Returns(20); // Simula 10% de imposto sobre 200 total (apenas exemplo)

        _factoryMock.Setup(f => f.ObterCalculoImpostoAtivoAsync())
            .ReturnsAsync(impostoMock.Object);

        // Act
        var result = await _pedidoService.ProcessarPedidoAsync(request);

        // Assert
        // Verifica se obteve a estratégia de imposto
        _factoryMock.Verify(f => f.ObterCalculoImpostoAtivoAsync(), Times.Once);

        // Verifica se o imposto foi calculado na entidade (indiretamente, verificando o valor final se possível, mas aqui verificamos as chamadas)
        // O método CalcularImposto é void, mas altera o estado do pedido.
        // Verificamos se o repositório foi chamado com o pedido contendo o imposto calculado (se possível verificar o argumento)
        
        _repositoryMock.Verify(r => r.CriarAsync(It.Is<Pedido>(p => 
            p.PedidoId == request.PedidoId &&
            p.ClienteId == request.ClienteId &&
            p.Itens.Count == 2
        )), Times.Once);

        // Verifica envio para fila
        _publisherMock.Verify(p => p.Publicar(It.Is<Pedido>(p => p.PedidoId == request.PedidoId)), Times.Once);

        // Verifica retorno
        Assert.NotNull(result);
        Assert.Equal(request.PedidoId, result.Id);
    }
}
