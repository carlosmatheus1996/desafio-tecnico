using MongoDB.Driver;
using ProdutoExternoA.Domain;
using ProdutoExternoA.MessageException;

namespace ProdutoExternoA.Infrastructure
{
    public interface IPedidoRepository
    {
        Task CriarAsync(Pedido pedido);
        Task<Pedido?> ObterPorIdAsync(long idExterno);
        Task<IEnumerable<Pedido>> ObterTodosAsync(string? status = null);
    }

    public class PedidoRepository : IPedidoRepository
    {
        private readonly IMongoCollection<Pedido> _collection;

        public PedidoRepository(IMongoDatabase db)
        {
            _collection = db.GetCollection<Pedido>("pedidos");
            var index = Builders<Pedido>.IndexKeys.Ascending(x => x.PedidoId);
            _collection.Indexes.CreateOne(new CreateIndexModel<Pedido>(index, new CreateIndexOptions { Unique = true }));
        }

        public async Task CriarAsync(Pedido pedido)
        {
            try
            {
                await _collection.InsertOneAsync(pedido);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new PedidoDuplicadoException($"Pedido {pedido.PedidoId} já registrado.");
            }
        }

        public async Task<Pedido?> ObterPorIdAsync(long idExterno) =>
            await _collection.Find(x => x.PedidoId == idExterno).FirstOrDefaultAsync();

        public async Task<IEnumerable<Pedido>> ObterTodosAsync(string? status = null)
        {
            var filtro = Builders<Pedido>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(status))
            {
                filtro = Builders<Pedido>.Filter.Eq(x => x.Status, status);
            }

            return await _collection.Find(filtro).ToListAsync();
        }
    }
}