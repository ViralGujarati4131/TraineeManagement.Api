using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TraineeManagement.Api.Contract.SubmissionProcessingContarct;
using TraineeManagement.Api.Messaging.RabbitMqConnection;
using RabbitMQ.Client;
using TraineeManagement.Api.Data.Constants;

namespace TraineeManagement.Api.Messaging.RabbitMQPublisher;

public class RabbitMqService
{
    private readonly RabbitConnection _connection;

    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(RabbitConnection connection, ILogger<RabbitMqService> logger)
    {
        _connection = connection;
        _logger = logger;
    }
    public async Task PublishAsync(SubmissionProcessingContract message)
    {
        if (_connection.Channel == null)
        {
            _logger.LogError("RabbitMQ channel is not initialized.");
            return;
        }

        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        BasicProperties properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent
        };

        await _connection.Channel.BasicPublishAsync(
            exchange: "",
            routingKey: AppConstants.RabbitMQ.SubmissionProcessing,
            mandatory: false,
            basicProperties: properties,
            body: body
        );

        _logger.LogInformation(
            "Published message: MessageId={MessageId}, CorrelationId={CorrelationId}, SubmissionFileId={SubmissionFileId}",
            message.MessageId, message.CorrelationId, message.SubmissionFileId
        );
    }
}
