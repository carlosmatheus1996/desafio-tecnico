using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProdutoExternoA.Service;

namespace ProdutoExternoA.Domain
{
    public class Pedido
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public long PedidoId { get; private set; }
        public long ClienteId { get; private set; }
        public List<PedidoItem> Itens { get; private set; } = new();
        public decimal ValorImposto { get; private set; }
        public string Status { get; private set; }
        public DateTime DataCriacao { get; private set; }

        public Pedido(long pedidoId, long clienteId, List<PedidoItem> itens)
        {
            Id = Guid.NewGuid();
            PedidoId = pedidoId;
            ClienteId = clienteId;
            Itens = itens;
            Status = "Criado";
            DataCriacao = DateTime.UtcNow;
        }

        public void CalcularImposto(ICalculadoraImpostoStrategy imposto)
        {
            ValorImposto = imposto.Calcular(Itens.Sum(x => x.Valor * x.Quantidade));
        }
    }

    public record PedidoItem(long ProdutoId, int Quantidade, decimal Valor);
}