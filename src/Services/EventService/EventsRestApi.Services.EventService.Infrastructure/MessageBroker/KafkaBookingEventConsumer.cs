using Confluent.Kafka;
using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Repositories;
using EventsRestApi.Services.SharedContracts.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventsRestApi.Services.EventService.Infrastructure.MessageBroker
{
    public class KafkaBookingEventConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger<KafkaBookingEventConsumer> _logger;

        private readonly TimeProvider _timeProvider;

        private readonly ConsumerConfig _consumerConfig;

        public KafkaBookingEventConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<KafkaBookingEventConsumer> logger,
            TimeProvider timeProvider)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _timeProvider = timeProvider;

            var bootstrapServers = configuration["Kafka:BootstrapServers"];

            if (string.IsNullOrEmpty(bootstrapServers))
            {
                throw new InvalidOperationException("Настройка Kafka:BootstrapServers обязательно должна присутствовать в конфигурационном файле.");
            }

            var groupId = configuration["Kafka:GroupId"];

            if (string.IsNullOrEmpty(groupId))
            {
                throw new InvalidOperationException("Настройка Kafka:GroupId обязательно должна присутствовать в конфигурационном файле.");
            }

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private void StartConsumerLoop(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            consumer.Subscribe(new[] { BookingTopics.BookingConfirmed, BookingTopics.BookingCancelled });

            _logger.LogInformation(
                "Подписчик Kafka запущен на топики: '{Confirmed}', '{Cancelled}'",
                BookingTopics.BookingConfirmed,
                BookingTopics.BookingCancelled);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult == null)
                    {
                        continue;
                    }

                    bool isProcessedSuccessfully;

                    // Маршрутизация в зависимости от того, из какого топика пришло сообщение
                    if (consumeResult.Topic == BookingTopics.BookingConfirmed)
                    {
                        isProcessedSuccessfully = ProcessBookingConfirmation(consumeResult.Message.Value, stoppingToken)
                            .GetAwaiter().GetResult();
                    }
                    else if (consumeResult.Topic == BookingTopics.BookingCancelled)
                    {
                        isProcessedSuccessfully = ProcessBookingCancellation(consumeResult.Message.Value, stoppingToken)
                            .GetAwaiter().GetResult();
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Получено сообщение из неизвестного топика: {Topic}",
                            consumeResult.Topic);

                        isProcessedSuccessfully = true;
                    }

                    if (isProcessedSuccessfully)
                    {
                        consumer.Commit(consumeResult);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Непредвиденная ошибка в цикле обработки Kafka-сообщений.");

                    Thread.Sleep(2000);
                }
            }

            consumer.Close();
        }

        private async Task<bool> ProcessBookingConfirmation(string rawJson, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            BookingConfirmedEvent? bookingConfirmedEvent;
            try
            {
                bookingConfirmedEvent = JsonSerializer.Deserialize<BookingConfirmedEvent>(rawJson);

                if (bookingConfirmedEvent == null)
                {
                    return true;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Не удалось десериализовать сообщение: {RawJson}",
                    rawJson);

                return true;
            }

            try
            {
                var foundEvent = await eventRepository.GetByIdAsync(bookingConfirmedEvent.EventId, cancellationToken);

                if (foundEvent == null)
                {
                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: событие с Id: {EventId} не существует.",
                        _timeProvider.GetUtcNow().UtcDateTime,
                        bookingConfirmedEvent.BookingId,
                        bookingConfirmedEvent.EventId);

                    return true;
                }

                if (foundEvent.IsFinished(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: событие с Id: {EventId} уже завершилось.",
                        _timeProvider.GetUtcNow().UtcDateTime,
                        bookingConfirmedEvent.BookingId,
                        bookingConfirmedEvent.EventId);

                    return true;
                }

                if (foundEvent.IsStarted(_timeProvider.GetUtcNow().UtcDateTime))
                {
                    _logger.LogWarning(
                        "{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: событие с Id: {EventId} уже началось.",
                        _timeProvider.GetUtcNow().UtcDateTime,
                        bookingConfirmedEvent.BookingId,
                        bookingConfirmedEvent.EventId);

                    return true;
                }

                if (!foundEvent.TryReserveSeats(bookingConfirmedEvent.BookedSeats))
                {
                    _logger.LogWarning(
                        @"{ProcessedAt}: Бронирование с Id: {BookingId} отклонено: на событие с Id: {EventId} осталось только {AvailableSeats} мест,
                        а бронирование создано на {BookedSeats} мест.",
                        _timeProvider.GetUtcNow().UtcDateTime,
                        bookingConfirmedEvent.BookingId,
                        bookingConfirmedEvent.EventId,
                        foundEvent.AvailableSeats,
                        bookingConfirmedEvent.BookedSeats);

                    return true;
                }

                await eventRepository.SaveContextAsync(cancellationToken);

                _logger.LogInformation(
                    @"{ProcessedAt}: Бронирование с Id: {BookingId} подтверждено: 
                        на событие с Id: {EventId} забронировано {BookedSeats} мест, остаток мест: {AvailableSeats}.",
                    _timeProvider.GetUtcNow().UtcDateTime,
                    bookingConfirmedEvent.BookingId,
                    bookingConfirmedEvent.EventId,
                    bookingConfirmedEvent.BookedSeats,
                    foundEvent.AvailableSeats);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{ProcessedAt}: Непредвиденная ошибка в процессе обработки бронирования с Id: {BookingId} на событие с Id: {EventId}",
                    _timeProvider.GetUtcNow().UtcDateTime,
                    bookingConfirmedEvent.BookingId,
                    bookingConfirmedEvent.EventId);

                return false;
            }
        }

        private async Task<bool> ProcessBookingCancellation(string rawJson, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            BookingCancelledEvent? bookingCancelledEvent;
            try
            {
                bookingCancelledEvent = JsonSerializer.Deserialize<BookingCancelledEvent>(rawJson);

                if (bookingCancelledEvent == null)
                {
                    return true;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Не удалось десериализовать сообщение: {RawJson}",
                    rawJson);

                return true;
            }

            try
            {
                var foundEvent = await eventRepository.GetByIdAsync(bookingCancelledEvent.EventId, cancellationToken);

                if (foundEvent == null)
                {
                    _logger.LogWarning(
                        @"{ProcessedAt}: Невозможно вернуть места на событие после отмены бронирования с Id: {BookingId}: 
                            событие с Id: {EventId} не существует.",
                        _timeProvider.GetUtcNow().UtcDateTime,
                        bookingCancelledEvent.BookingId,
                        bookingCancelledEvent.EventId);

                    return true;
                }

                foundEvent.ReleaseSeats(bookingCancelledEvent.BookedSeats);
                await eventRepository.SaveContextAsync(cancellationToken);

                _logger.LogInformation(
                    @"{ProcessedAt}: После отмены бронирования с Id: {BookingId} на событие с Id: {EventId} возвращено {BookedSeats} мест.",
                    _timeProvider.GetUtcNow().UtcDateTime,
                    bookingCancelledEvent.BookingId,
                    bookingCancelledEvent.EventId,
                    bookingCancelledEvent.BookedSeats);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{ProcessedAt}: Непредвиденная ошибка в процессе отмены бронирования с Id: {BookingId} на событие с Id: {EventId}",
                    _timeProvider.GetUtcNow().UtcDateTime,
                    bookingCancelledEvent.BookingId,
                    bookingCancelledEvent.EventId);

                return false;
            }
        }
    }
}
