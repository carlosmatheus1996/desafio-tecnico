using ProdutoExternoA.Controllers;
using ProdutoExternoA.Domain;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ProdutoExternoA.Infrastructure
{
    public interface IRabbitMqPublisher
    {
        Task Publicar(Pedido pedido);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ILogger<PedidoController> _logger;
        private readonly ConnectionFactory _factory;
        private const string _queueName = "pedidos-processados-sistema-b";

        public RabbitMqPublisher(IConfiguration configuration, ILogger<PedidoController> logger)
        {
            var host = configuration["RabbitMq:Host"] ?? "localhost";
            _factory = new ConnectionFactory { HostName = host };
            _logger = logger;
        }

        public async Task Publicar(Pedido pedido)
        {
            try
            {
                _logger.LogInformation("Publicando evento de Pedido Criado: {PedidoId}", pedido.PedidoId);

                await using var connection = await _factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var json = JsonSerializer.Serialize(pedido);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: _queueName,
                    body: body);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Falha ao publicar evento do pedido {PedidoId} no RabbitMQ", pedido.PedidoId);
                throw;
            }
        }
    }
}