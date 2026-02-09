using ProdutoExternoA.Domain;
using ProdutoExternoA.Infrastructure;

namespace ProdutoExternoA.Service
{
    public record CriarPedidoRequest(long PedidoId, long ClienteId, List<PedidoItem> Itens);
    public record PedidoResponse(long Id, string Status);

    public interface IPedidoService
    {
        Task<PedidoResponse> ProcessarPedidoAsync(CriarPedidoRequest request);
    }

    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _repository;
        private readonly ICalculadoraImpostoFactory _factory;
        private readonly IRabbitMqPublisher _publisher;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(
            IPedidoRepository repository,
            ICalculadoraImpostoFactory factory,
            IRabbitMqPublisher publisher,
            ILogger<PedidoService> logger)
        {
            _repository = repository;
            _factory = factory;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<PedidoResponse> ProcessarPedidoAsync(CriarPedidoRequest request)
        {
            var itensDomain = request.Itens.Select(i => new PedidoItem(i.ProdutoId, i.Quantidade, i.Valor)).ToList();
            var pedido = new Pedido(request.PedidoId, request.ClienteId, itensDomain);

            var imposto = await _factory.ObterCalculoImpostoAtivoAsync();

            pedido.CalcularImposto(imposto);

            await _repository.CriarAsync(pedido);

            await _publisher.Publicar(pedido);

            _logger.LogInformation("Pedido {Id} processado. Imposto: {Imposto}", pedido.PedidoId, pedido.ValorImposto);

            return new PedidoResponse(pedido.PedidoId, pedido.Status);
        }
    }

}