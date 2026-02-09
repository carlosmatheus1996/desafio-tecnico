using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProdutoExternoA.Domain;
using ProdutoExternoA.Infrastructure;
using ProdutoExternoA.MessageException;
using ProdutoExternoA.Service;

namespace ProdutoExternoA.Tests.Service
{
    public class PedidoServiceTests
    {
        private readonly IPedidoRepository _repository;
        private readonly ICalculadoraImpostoFactory _factory;
        private readonly IRabbitMqPublisher _publisher;
        private readonly ILogger<PedidoService> _logger;
        private readonly PedidoService _service;

        public PedidoServiceTests()
        {
            _repository = Substitute.For<IPedidoRepository>();
            _factory = Substitute.For<ICalculadoraImpostoFactory>();
            _publisher = Substitute.For<IRabbitMqPublisher>();
            _logger = Substitute.For<ILogger<PedidoService>>();

            _service = new PedidoService(_repository, _factory, _publisher, _logger);
        }

        [Fact]
        public async Task ProcessarPedidoAsync_DadosValidos_DeveSalvarEPublicarComSucesso()
        {
            var request = CriarRequestValido();

            var strategyMock = Substitute.For<ICalculadoraImpostoStrategy>();
            strategyMock.Calcular(Arg.Any<decimal>()).Returns(10.0m); 

            _factory.ObterCalculoImpostoAtivoAsync().Returns(strategyMock);

            var resultado = await _service.ProcessarPedidoAsync(request);

            resultado.Should().NotBeNull();
            resultado.Status.Should().Be("Criado");
            resultado.Id.Should().Be(request.PedidoId);

            strategyMock.Received(1).Calcular(Arg.Any<decimal>());

            await _repository.Received(1).CriarAsync(Arg.Is<Pedido>(p =>
                p.PedidoId == request.PedidoId &&
                p.ValorImposto == 10.0m));

            await _publisher.Received(1).Publicar(Arg.Is<Pedido>(p =>
                p.PedidoId == request.PedidoId));
        }

        [Fact]
        public async Task ProcessarPedidoAsync_QuandoRepositorioFalhar_DeveLancarExcecaoENaoPublicar()
        {
            var request = CriarRequestValido();

            var strategyMock = Substitute.For<ICalculadoraImpostoStrategy>();
            _factory.ObterCalculoImpostoAtivoAsync().Returns(strategyMock);

            _repository
                .When(x => x.CriarAsync(Arg.Any<Pedido>()))
                .Do(x => throw new PedidoDuplicadoException("Pedido duplicado"));

            Func<Task> acao = async () => await _service.ProcessarPedidoAsync(request);

            await acao.Should().ThrowAsync<PedidoDuplicadoException>()
                .WithMessage("Pedido duplicado");

            await _publisher.DidNotReceive().Publicar(Arg.Any<Pedido>());
        }

        [Fact]
        public async Task ProcessarPedidoAsync_QuandoPublisherFalhar_DeveLancarExcecao()
        {
            var request = CriarRequestValido();

            var strategyMock = Substitute.For<ICalculadoraImpostoStrategy>();
            _factory.ObterCalculoImpostoAtivoAsync().Returns(strategyMock);

            _publisher
                .When(x => x.Publicar(Arg.Any<Pedido>()))
                .Do(x => throw new Exception("RabbitMQ fora do ar"));

            Func<Task> acao = async () => await _service.ProcessarPedidoAsync(request);

            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("RabbitMQ fora do ar");

            await _repository.Received(1).CriarAsync(Arg.Any<Pedido>());
        }

        private CriarPedidoRequest CriarRequestValido()
        {
            return new CriarPedidoRequest(
                PedidoId: 12345,
                ClienteId: 999,
                Itens: new List<PedidoItem>
                {
                    new PedidoItem(1, 2, 50.0m), 
                    new PedidoItem(2, 1, 30.0m) 
                }
            );
        }
    }
}