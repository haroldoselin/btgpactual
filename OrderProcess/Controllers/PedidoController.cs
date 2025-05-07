using Microsoft.AspNetCore.Mvc;
using OrderProcess.Domain;
using OrderProcess.Infrastructure.Repositories;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderProcess.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IModel _channel;

        public PedidoController(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;

            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            var factory = new ConnectionFactory
            {
                HostName = "172.17.0.2", //ip do docker
                Port = 5672,
                UserName = "guest",
                Password = "guest",
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            //_channel.QueueDeclare("pedidosQueue", durable: false, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(queue: "pedidosQueue", durable: true, exclusive: false, autoDelete: false);
        }

        [HttpPost("gravar-pedido")]
        public async Task<IActionResult> GravarPedido(Pedido pedidos)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pedidos));
            _channel.BasicPublish("", "pedidosQueue", null, body);
            return Accepted();
        }

        [HttpGet("{codigoPedido}/valor-total")]
        public async Task<IActionResult> ObterValorTotal(int codigoPedido)
        {
            var pedido = await _pedidoRepository.ObterPedidoAsync(codigoPedido);
            if (pedido == null)
                return NotFound("Pedido não encontrado.");

            return Ok(new { codigoPedido, valorTotal = pedido.ValorTotal });
        }

        [HttpGet("cliente/{codigoCliente}/quantidade")]
        public async Task<IActionResult> ObterQuantidadePedidos(int codigoCliente)
        {
            var quantidade = await _pedidoRepository.ObterQuantidadePedidosPorClienteAsync(codigoCliente);
            return Ok(new { codigoCliente, quantidadePedidos = quantidade });
        }

        [HttpGet("cliente/{codigoCliente}/lista")]
        public async Task<IActionResult> ObterListaPedidos(int codigoCliente)
        {
            var pedidos = await _pedidoRepository.ObterPedidosPorClienteAsync(codigoCliente);
            return Ok(pedidos);
        }
    }
}
