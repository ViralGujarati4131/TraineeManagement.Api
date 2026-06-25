using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TraineeManagement.Api.Messaging.RabbitMqConnectionSettings;

namespace TraineeManagement.Api.Messaging.RabbitMqConnection
{
    public class RabbitConnection : IAsyncDisposable
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ILogger<RabbitConnection> _logger;
        private readonly RabbitMqSettings _settings;
        private readonly ConnectionFactory _factory;

        public IChannel? Channel => _channel;

        public RabbitConnection(IOptions<RabbitMqSettings> options, ILogger<RabbitConnection> logger)
        {
            _logger = logger;
            _settings = options.Value;

            _factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                VirtualHost = _settings.VirtualHost,
                UserName = _settings.Username,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true
            };
        }

        public async Task InitializeAsync()
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            _logger.LogInformation("RabbitMQ base connection and channel successfully established.");
        }

        public async Task RegisterQueueAsync(string queueName)
        {
            if (_channel is null) throw new InvalidOperationException("Channel is not initialized.");

            await _channel.QueueDeclareAsync(   
                queue: queueName,
                durable: true,       
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null && _channel.IsOpen)
                await _channel.CloseAsync();

            if (_connection != null && _connection.IsOpen)
                await _connection.CloseAsync();

            _logger.LogInformation("RabbitMQ async connection closed.");
        }
    }
}