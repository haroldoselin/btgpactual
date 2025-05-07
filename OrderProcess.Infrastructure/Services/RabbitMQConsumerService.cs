using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcess.Domain;
using OrderProcess.Infrastructure.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderProcess.Infrastructure.Services
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly IPedidoRepository _pedidoRepository;
        private IConnection _connection;
        private IModel _channel;
        private const string QueueName = "pedidosQueue";

        public RabbitMQConsumerService(ILogger<RabbitMQConsumerService> logger, IPedidoRepository pedidoRepository)
        {
            _logger = logger;
            _pedidoRepository = pedidoRepository;
            CriarConexao();
        }

        private void CriarConexao()
        {
            var settings = new RabbitMQSettings();

            var factory = new ConnectionFactory
            {
                HostName = settings.HostName, 
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var mensagem = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Mensagem recebida: {mensagem}", mensagem);

                try
                {
                    var pedido = JsonSerializer.Deserialize<Pedido>(mensagem);
                    if (pedido != null)
                    {
                        await _pedidoRepository.InserirPedidoAsync(pedido);
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError("Erro ao desserializar a mensagem: {erro}", ex.Message);
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }

        public class RabbitMQSettings
        {
            public string HostName { get; set; } = "172.17.0.2";
            public int Port { get; set; } = 5672;
            public string UserName { get; set; } = "guest";
            public string Password { get; set; } = "guest";
            public string QueueName { get; set; } = "pedidosQueue";
        }
    }
}
