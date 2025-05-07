using MongoDB.Driver;
using OrderProcess.Domain;

namespace OrderProcess.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly IMongoCollection<Pedido> _pedidosMongoDb;

        public PedidoRepository(IMongoDatabase mongoDb)
        {
            //var database = client.GetDatabase("OrderDB");
            _pedidosMongoDb = mongoDb.GetCollection<Pedido>("Pedidos");
        }

        public async Task InserirPedidoAsync(Pedido pedido)
        {
            await _pedidosMongoDb.InsertOneAsync(pedido);
        }

        public async Task<Pedido> ObterPedidoAsync(int codigoPedido)
        {
            return await _pedidosMongoDb.Find(p => p.CodigoPedido == codigoPedido).FirstOrDefaultAsync();
        }

        public async Task<List<Pedido>> ObterPedidosPorClienteAsync(int codigoCliente)
        {
            return await _pedidosMongoDb.Find(p => p.CodigoCliente == codigoCliente).ToListAsync();
        }

        public async Task<int> ObterQuantidadePedidosPorClienteAsync(int codigoCliente)
        {
            return (int)await _pedidosMongoDb.CountDocumentsAsync(p => p.CodigoCliente == codigoCliente);
        }
    }
}
