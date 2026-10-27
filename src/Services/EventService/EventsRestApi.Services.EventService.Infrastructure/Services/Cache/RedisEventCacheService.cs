using EventsRestApi.Services.EventService.Application.InterfacesForInfrastructure.Cache;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Net.Sockets;

namespace EventsRestApi.Services.EventService.Infrastructure.Services.Cache
{
    public class RedisEventCacheService : IEventCacheService
    {
        private readonly IDatabase? _database;
        private readonly ILogger<RedisEventCacheService> _logger;

        public RedisEventCacheService(IConnectionMultiplexer redis, ILogger<RedisEventCacheService> _logger)
        {
            this._logger = _logger;

            try
            {
                _database = redis.GetDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при получении базы данных Redis во время инициализации.");
                _database = null;
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            if (_database == null)
            {
                return null;
            }

            try
            {
                return await _database.StringGetAsync(key);
            }
            catch (Exception ex)
                when (ex is RedisException or TimeoutException or SocketException)
            {
                _logger.LogWarning(
                    ex,
                    "Redis недоступен при попытке получить ключ: {Key}. Запрос пойдет в БД.",
                    key);

                return null;
            }
        }

        public async Task SetAsync(string key, string value, TimeSpan expiration)
        {
            if (_database == null)
            {
                return;
            }

            try
            {
                await _database.StringSetAsync(key, value, expiration);
            }
            catch (Exception ex)
                when (ex is RedisException or TimeoutException or SocketException)
            {
                _logger.LogWarning(
                    ex,
                    "Redis недоступен при попытке записать ключ: {Key}.",
                    key);
            }
        }

        public async Task DeleteAsync(string key)
        {
            if (_database == null)
            {
                return;
            }

            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
                when (ex is RedisException or TimeoutException or SocketException)
            {
                _logger.LogWarning(
                    ex,
                    "Redis недоступен при попытке удалить ключ: {Key}.",
                    key);
            }
        }
    }
}
