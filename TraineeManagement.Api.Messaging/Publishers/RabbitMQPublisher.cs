using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TraineeManagement.Api.Contract.SubmissionProcessingContarct;
using TraineeManagement.Api.Messaging.RabbitMqConnection;
using RabbitMQ.Client;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.Data.CustomException;

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
        IConnection? connection = _connection.Connection;

        if (connection == null || !connection.IsOpen)
        {
            _logger.LogError(
                "Dependency failure: RabbitMQ publish failed. Reason: {Reason}. CorrelationId: {CorrelationId}",
                "Connection unavailable",
                message.CorrelationId);
            return;throw new OperationException(CustomResponse.ChannelNotInitialized);
        }

        CreateChannelOptions channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        );

        await using IChannel channel = await connection.CreateChannelAsync(channelOptions);

        if (channel == null)
        {
            _logger.LogError(
                "Dependency failure: RabbitMQ channel creation failed. Reason: {Reason}. CorrelationId: {CorrelationId}",
                "Channel unavailable",
                message.CorrelationId);
            throw new OperationException(CustomResponse.ChannelNotInitialized);
        }

        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        BasicProperties properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            CorrelationId = message.CorrelationId.ToString(),
            MessageId = message.MessageId.ToString()
        };

        string targetExchange = AppConstants.RabbitMQ.GetExchange(AppConstants.RabbitMQ.SubmissionProcessing);
        string targetRoutingKey = AppConstants.RabbitMQ.GetRoutingKey(AppConstants.RabbitMQ.SubmissionProcessing);

        try
        {
            await channel.BasicPublishAsync(
                exchange: targetExchange,
                routingKey: targetRoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation(
                "Publish success. MessageId: {MessageId}, CorrelationId: {CorrelationId}, Exchange: {Exchange}, RoutingKey: {RoutingKey}, SubmissionFileId: {SubmissionFileId}",
                message.MessageId,
                message.CorrelationId,
                targetExchange,
                targetRoutingKey,
                message.SubmissionFileId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Publish failed. MessageId: {MessageId}, CorrelationId: {CorrelationId}, Error: {ErrorMessage}",
                message.MessageId,
                message.CorrelationId,
                ex.Message);
            throw;
        }
    }
}