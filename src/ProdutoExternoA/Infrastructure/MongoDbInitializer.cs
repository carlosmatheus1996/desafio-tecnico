using MongoDB.Driver;
using ProdutoExternoA.Domain;

namespace ProdutoExternoA.Infrastructure
{
    public class MongoDbInitializer
    {
        private readonly IMongoDatabase _database;

        public MongoDbInitializer(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task InicializarAsync()
        {
            var collection = _database.GetCollection<Pedido>("pedidos");

            var indexKeyId = Builders<Pedido>.IndexKeys.Ascending(x => x.PedidoId);
            var indexModelId = new CreateIndexModel<Pedido>(indexKeyId, new CreateIndexOptions { Unique = true });

            var indexKeyStatus = Builders<Pedido>.IndexKeys.Ascending(x => x.Status);
            var indexModelStatus = new CreateIndexModel<Pedido>(indexKeyStatus);

            await collection.Indexes.CreateManyAsync(new[] { indexModelId, indexModelStatus });
        }
    }
}