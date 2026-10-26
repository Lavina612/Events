using Confluent.Kafka;
using EventsRestApi.Services.BookingService.Application.InterfacesForInfrastructure.MessageBroker;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventsRestApi.Services.BookingService.Infrastructure.MessageBroker
{
    public class KafkaEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        private readonly ILogger<KafkaEventPublisher> _logger;

        public KafkaEventPublisher(
            IConfiguration configuration, 
            ILogger<KafkaEventPublisher> logger)
        {
            _logger = logger;

            var bootstrapServers = configuration["Kafka:BootstrapServers"];

            if (string.IsNullOrEmpty(bootstrapServers))
            {
                throw new InvalidOperationException("Настройка Kafka:BootstrapServers обязательно должна присутствовать в конфигурационном файле.");
            }

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<T>(string topic, string messageKey, T message, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(message);

                var kafkaMessage = new Message<string, string>
                {
                    Key = messageKey,
                    Value = jsonPayload
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

                _logger.LogInformation(
                    "Событие {EventType} успешно опубликовано в топик {Topic} (Раздел: {Partition}, Смещение: {Offset}). Ключ: {Key}",
                    typeof(T).Name,
                    topic,
                    result.Partition.Value,
                    result.Offset.Value,
                    messageKey);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка публикации события {EventType} в Kafka (Топик: {Topic}, Ключ: {Key}). Код ошибки: {ErrorCode}",
                    typeof(T).Name,
                    topic,
                    messageKey,
                    ex.Error.Code);

                throw;
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}
