using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyRustInventory.Application.Common.Interfaces;


namespace MyRustInventory.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private ILogger _logger;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public T GetData<T>(string key)
        {
            try
            {
                T item = (T)_memoryCache.Get(key);
                return item;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error {e.Message}");
                throw;
            }
        }

        public object RemoveData(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _memoryCache.Remove(key);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error {e.Message}");
                throw;
            }
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            bool res = true;
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _memoryCache.Set(key, value, expirationTime);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error {e.Message}");
                throw;
            }
            return res;
        }
    }
}
