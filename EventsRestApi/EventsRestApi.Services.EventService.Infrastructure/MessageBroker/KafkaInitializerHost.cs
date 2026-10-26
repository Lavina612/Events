using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventsRestApi.Services.SharedContracts.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventsRestApi.Services.EventService.Infrastructure.MessageBroker
{
    public class KafkaInitializerHost : IHostedService
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<KafkaInitializerHost> _logger;

        public KafkaInitializerHost(
            IConfiguration configuration, 
            ILogger<KafkaInitializerHost> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];

            if (string.IsNullOrEmpty(bootstrapServers))
            {
                throw new InvalidOperationException("Настройка Kafka:BootstrapServers обязательно должна присутствовать в конфигурационном файле.");
            }

            var config = new AdminClientConfig()
            { 
                BootstrapServers = bootstrapServers 
            };

            using var adminClient = new AdminClientBuilder(config).Build();

            var topics = new[] { BookingTopics.BookingConfirmed };

            foreach (var topic in topics)
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new[]
                    {
                        new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }
                    });

                    _logger.LogInformation("Топик '{Topic}' успешно создан.", topic);
                }
                catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    _logger.LogInformation("Топик '{Topic}' уже существует, поэтому его можно пропустить.", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Не удалось создать топик '{Topic}' при старте.", topic);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
