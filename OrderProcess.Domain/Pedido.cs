namespace OrderProcess.Domain
{
    public class Pedido
    {
        public int CodigoPedido { get; set; }
        public int CodigoCliente { get; set; }
        public List<PedidoItem> Itens { get; set; } = [];
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        public decimal ValorTotal => Itens.Sum(i => i.Quantidade * i.Preco);
    }
}
