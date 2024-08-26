namespace Shonos.FluentCache.Abstractions
{
    /// <summary>
    /// Represents a fluent cache interface for configuring cache operations.
    /// </summary>
    public interface IFluentCache
    {
        /// <summary>
        /// Specifies an action to perform when cache miss occurs.
        /// This also creates the typed fluent cache interface
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="action">The action to execute on cache miss.</param>
        /// <returns>An interface for typed fluent cache operations.</returns>
        IFluentCache<T> CreateWithOnCacheMiss<T>(Func<Task<T>> action);

        /// <summary>
        /// Deletes a cached entry based on the specified key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a fluent cache interface for configuring typed cache operations.
    /// </summary>
    /// <typeparam name="T">The type of the cache value.</typeparam>
    public interface IFluentCache<T>
    {
        /// <summary>
        /// Specifies a predicate to determine whether to cache the result.
        /// </summary>
        /// <param name="predicate">The predicate to evaluate.</param>
        /// <returns>An interface for fluent cache operations.</returns>
        IFluentCache<T> CacheIf(Func<T, bool> predicate);

        /// <summary>
        /// Sets an absolute expiration relative to now for the cache entry.
        /// </summary>
        /// <param name="duration">The duration of the expiration relative to now.</param>
        /// <returns>An interface for fluent cache operations.</returns>
        IFluentCache<T> SetAbsoluteExpirationRelativeToNow(TimeSpan duration);

        /// <summary>
        /// Sets an absolute expiration for the cache entry.
        /// </summary>
        /// <param name="offset">The absolute expiration time.</param>
        /// <returns>An interface for fluent cache operations.</returns>
        IFluentCache<T> SetAbsoluteExpiration(DateTimeOffset offset);

        /// <summary>
        /// Sets a sliding expiration for the cache entry.
        /// </summary>
        /// <param name="duration">The duration of the sliding expiration.</param>
        /// <returns>An interface for fluent cache operations.</returns>
        IFluentCache<T> SetSlidingExpiration(TimeSpan duration);

        /// <summary>
        /// Asynchronously retrieves a cached value based on the specified key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with the cached value.</returns>
        Task<T?> GetAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously sets a value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to be cached.</param>
        /// <param name="absoluteExpiration">The absolute expiration time for the cache entry.</param>
        /// <param name="slidingExpiration">The sliding expiration time for the cache entry.</param>
        /// <param name="absoluteExpirationRelativeToNow">The absolute expiration relative to now for the cache entry.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with a boolean indicating success or failure.</returns>
        Task<bool> SetAsync(string key, T value,
            DateTimeOffset? absoluteExpiration = default,
            TimeSpan? slidingExpiration = default,
            TimeSpan? absoluteExpirationRelativeToNow = default,
            CancellationToken cancellationToken = default);
    }
}
