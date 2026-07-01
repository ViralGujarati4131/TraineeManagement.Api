using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TraineeManagement.Api.Messaging.RabbitMqConnectionSettings;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.Data.CustomException;

namespace TraineeManagement.Api.Messaging.RabbitMqConnection
{
    public class RabbitConnection : IAsyncDisposable
    {
        private IConnection? _connection;    
        private readonly ILogger<RabbitConnection> _logger;
        private readonly RabbitMqSettings _settings;
        private readonly ConnectionFactory _factory;
        public IConnection? Connection => _connection;

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
            try
            {
                _connection = await _factory.CreateConnectionAsync();
                _logger.LogInformation("Publish/consume connection initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dependency failure opening rabbitmq connection.");
                throw new OperationException(CustomResponse.ConnectionNotInitialized);
            }
        }

        public async Task RegisterQueueAsync(string queueName)
        {
            if(_connection == null)
            {
                _logger.LogError("Dependency failure: RabbitMq connection uninitialized.");
                throw new OperationException(CustomResponse.ConnectionNotInitialized);
            }
                
            using IChannel _channel = await _connection.CreateChannelAsync();
            if (_channel is null) 
            {
                _logger.LogError("Dependency failure: RabbitMq channel allocation failed.");
                throw new OperationException(CustomResponse.ChannelNotInitialized);
            }

            string mainExchange = AppConstants.RabbitMQ.GetExchange(queueName);
            string mainQueue = AppConstants.RabbitMQ.GetQueue(queueName);
            string mainRoutingKey = AppConstants.RabbitMQ.GetRoutingKey(queueName);
            
            string deadLetterExchange = AppConstants.RabbitMQ.GetDlxExchange(queueName);
            string deadLetterQueue = AppConstants.RabbitMQ.GetDlxQueue(queueName);
            string deadLetterRoutingKey = AppConstants.RabbitMQ.GetDlxRoutingKey(queueName);

            _logger.LogInformation("RabbitMq initializing configuration pipeline");

            await _channel.ExchangeDeclareAsync(
                exchange: deadLetterExchange, 
                type: ExchangeType.Direct, 
                durable: true
            );
            
            await _channel.QueueDeclareAsync(
                queue: deadLetterQueue, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null
            );
            
            await _channel.QueueBindAsync(
                queue: deadLetterQueue, 
                exchange: deadLetterExchange, 
                routingKey: deadLetterRoutingKey
            );

            Dictionary<string, object?> mainQueueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", deadLetterExchange },
                { "x-dead-letter-routing-key", deadLetterRoutingKey }
            };

            await _channel.ExchangeDeclareAsync(
                exchange: mainExchange, 
                type: ExchangeType.Direct, 
                durable: true
            );

            await _channel.QueueDeclareAsync(
                queue: mainQueue, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: mainQueueArgs
            );
            
            await _channel.QueueBindAsync(
                queue: mainQueue, 
                exchange: mainExchange, 
                routingKey: mainRoutingKey
            );

            _logger.LogInformation("RabbitMq queue initialization successfully. TargetQueue: {Queue}", mainQueue);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
                _logger.LogInformation("State transition: RabbitMq connection closed asynchronously.");
            }
        }
    }
}