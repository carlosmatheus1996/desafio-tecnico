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
        private readonly ConnectionFactory _factory;
        private const string _queueName = "pedidos-processados-sistema-b";

        public RabbitMqPublisher(IConfiguration configuration)
        {
            var host = configuration["RabbitMq:Host"] ?? "localhost";
            _factory = new ConnectionFactory { HostName = host };
        }

        public async Task Publicar(Pedido pedido)
        {
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
    }
}