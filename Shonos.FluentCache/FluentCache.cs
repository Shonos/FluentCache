using Microsoft.Extensions.Caching.Distributed;
using Shonos.FluentCache.Abstractions;
using System.Text.Json;

namespace Shonos.FluentCache
{
    /// <summary>
    /// Implementation of IFluentCache
    /// OnCacheMiss sets the type
    /// </summary>
    public class FluentCache : IFluentCache
    {
        private readonly IDistributedCache _cache;

        public FluentCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public IFluentCache<T> CreateWithOnCacheMiss<T>(Func<Task<T>> action) => new FluentCache<T>(_cache, action);

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => _cache.RemoveAsync(key, cancellationToken);
    }
    /// <summary>
    /// Implementation of a typed IFluentCache using ICache
    /// </summary>
    public class FluentCache<T> : IFluentCache<T>
    {
        private readonly IDistributedCache _cache;
        private readonly Func<Task<T>>? _onCacheMiss;
        private Func<T, bool>? _cacheIfPredicate;

        private TimeSpan? _absoluteExpirationRelativeToNow;
        private DateTimeOffset? _absoluteExpiration;
        private TimeSpan? _slidingExpiration;

        public FluentCache(IDistributedCache cache, Func<Task<T>> onCacheMiss)
        {
            _cache = cache;
            _onCacheMiss = onCacheMiss;
            _absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // default cache expiration is 5 minutes 
        }

        public IFluentCache<T> CacheIf(Func<T, bool> predicate)
        {
            _cacheIfPredicate = predicate;
            return this;
        }

        public IFluentCache<T> SetAbsoluteExpirationRelativeToNow(TimeSpan duration)
        {
            // set other time variables to null to prevent conflict 
            _absoluteExpiration = null;
            _absoluteExpirationRelativeToNow = duration;
            _slidingExpiration = null;
            return this;
        }

        public IFluentCache<T> SetAbsoluteExpiration(DateTimeOffset offset)
        {
            // set other time variables to null to prevent conflict 
            _absoluteExpiration = offset;
            _absoluteExpirationRelativeToNow = null;
            _slidingExpiration = null;
            return this;
        }

        public IFluentCache<T> SetSlidingExpiration(TimeSpan duration)
        {
            // set other time variables to null to prevent conflict 
            _absoluteExpiration = null;
            _absoluteExpirationRelativeToNow = null;
            _slidingExpiration = duration;
            return this;
        }

        private async Task<T?> GetAsync<TValue>(string key, CancellationToken cancellationToken = default)
        {
            var resultInString = string.Empty;

            resultInString = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);

            return string.IsNullOrWhiteSpace(resultInString)
                ? default
                : JsonSerializer.Deserialize<T>(resultInString);
        }

        public async Task<T?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            var cachedData = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);

            if (cachedData == null && _onCacheMiss != null)
            {
                var result = await _onCacheMiss();
                if (result != null && (_cacheIfPredicate == null || _cacheIfPredicate(result)))
                {
                    await SetAsync(key, result,
                        absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow,
                        absoluteExpiration: _absoluteExpiration,
                        slidingExpiration: _slidingExpiration,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                return result;
            }

            return cachedData;
        }

        public async Task<bool> SetAsync(string key, T value,
            DateTimeOffset? absoluteExpiration = default,
            TimeSpan? slidingExpiration = default,
            TimeSpan? absoluteExpirationRelativeToNow = default,
            CancellationToken cancellationToken = default)
        {
            var valueInString = JsonSerializer.Serialize(value);

            try
            {
                await _cache.SetStringAsync(
                    key,
                    valueInString,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = absoluteExpiration,
                        AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
                        SlidingExpiration = slidingExpiration
                    },
                    cancellationToken).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
