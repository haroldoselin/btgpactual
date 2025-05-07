using OrderProcess.Domain;

namespace OrderProcess.Infrastructure.Repositories
{
    public interface IPedidoRepository
    {
        Task InserirPedidoAsync(Pedido pedido);
        Task<Pedido> ObterPedidoAsync(int codigoPedido);
        Task<List<Pedido>> ObterPedidosPorClienteAsync(int codigoCliente);
        Task<int> ObterQuantidadePedidosPorClienteAsync(int codigoCliente);

    }
}
