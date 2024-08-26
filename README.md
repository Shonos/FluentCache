# Shonos.FluentCache

`Shonos.FluentCache` is a library providing a fluent API for caching in .NET applications using `IDistributedCache`. It offers an easy-to-use interface for setting, retrieving, and managing cached data with flexible expiration options.

## Getting Started

### Installation

To use `Shonos.FluentCache`, you need to install the package from NuGet. You can do this via the NuGet Package Manager or by running the following command in your terminal:

```bash
dotnet add package Shonos.FluentCache
```


### Usage

Below is a guide to using the `Shonos.FluentCache` library, including the public methods provided by the `FluentCache` and `FluentCache<T>` classes.

## Classes

### FluentCache

This class implements `IFluentCache` and provides methods for creating a typed cache and removing items from the cache.

#### Methods

- **`FluentCache(IDistributedCache cache)`**
  - Constructor to initialize the cache with a distributed cache instance.
  - **Parameters:**
    - `cache`: An instance of `IDistributedCache` used for caching operations.

- **`IFluentCache<T> CreateWithOnCacheMiss<T>(Func<Task<T>> action)`**
  - Creates a typed cache instance that will execute the provided function when the cache misses.
  - **Parameters:**
    - `action`: A function that returns a `Task<T>` to be executed when the cache miss occurs.
  - **Returns:** An instance of `IFluentCache<T>`.

- **`Task RemoveAsync(string key, CancellationToken cancellationToken = default)`**
  - Removes an item from the cache by its key.
  - **Parameters:**
    - `key`: The key of the item to remove.
    - `cancellationToken`: A cancellation token to cancel the operation.
  - **Returns:** A `Task` representing the asynchronous operation.

### FluentCache<T>

This class implements `IFluentCache<T>` and provides methods for setting and retrieving cached items with type-specific operations.

#### Methods

- **`FluentCache(IDistributedCache cache, Func<Task<T>> onCacheMiss)`**
  - Constructor to initialize the typed cache with a distributed cache instance and a cache miss function.
  - **Parameters:**
    - `cache`: An instance of `IDistributedCache`.
    - `onCacheMiss`: A function that returns a `Task<T>` to be executed when the cache miss occurs.

- **`IFluentCache<T> CacheIf(Func<T, bool> predicate)`**
  - Configures a predicate to determine if the cached item should be stored based on its value.
  - **Parameters:**
    - `predicate`: A function that takes the cached item and returns a boolean indicating whether to cache the item.
  - **Returns:** The current `IFluentCache<T>` instance.

- **`IFluentCache<T> SetAbsoluteExpirationRelativeToNow(TimeSpan duration)`**
  - Sets the absolute expiration of the cached item relative to the current time.
  - **Parameters:**
    - `duration`: The duration relative to the current time after which the cache entry should expire.
  - **Returns:** The current `IFluentCache<T>` instance.

- **`IFluentCache<T> SetAbsoluteExpiration(DateTimeOffset offset)`**
  - Sets the absolute expiration of the cached item at a specific date and time.
  - **Parameters:**
    - `offset`: The date and time at which the cache entry should expire.
  - **Returns:** The current `IFluentCache<T>` instance.

- **`IFluentCache<T> SetSlidingExpiration(TimeSpan duration)`**
  - Sets the sliding expiration of the cached item.
  - **Parameters:**
    - `duration`: The duration after which the cache entry should expire if not accessed.
  - **Returns:** The current `IFluentCache<T>` instance.

- **`Task<T?> GetAsync(string key, CancellationToken cancellationToken = default)`**
  - Retrieves an item from the cache. If the item is not found, the cache miss function is invoked.
  - **Parameters:**
    - `key`: The key of the item to retrieve.
    - `cancellationToken`: A cancellation token to cancel the operation.
  - **Returns:** A `Task<T?>` representing the asynchronous operation, where the result is the cached item or `null` if not found and no result from the cache miss function.

- **`Task<bool> SetAsync(string key, T value, DateTimeOffset? absoluteExpiration = default, TimeSpan? slidingExpiration = default, TimeSpan? absoluteExpirationRelativeToNow = default, CancellationToken cancellationToken = default)`**
  - Sets an item in the cache with optional expiration settings.
  - **Parameters:**
    - `key`: The key under which the item will be stored.
    - `value`: The item to cache.
    - `absoluteExpiration`: The absolute expiration date and time.
    - `slidingExpiration`: The sliding expiration duration.
    - `absoluteExpirationRelativeToNow`: The duration relative to the current time after which the cache entry should expire.
    - `cancellationToken`: A cancellation token to cancel the operation.
  - **Returns:** A `Task<bool>` indicating the success or failure of the cache operation.

## Examples

### Basic Usage

```csharp
var cache = new FluentCache(myDistributedCache);
var typedCache = cache.CreateWithOnCacheMiss(async () => await FetchDataFromDatabase());

await typedCache
    .SetAbsoluteExpirationRelativeToNow(TimeSpan.FromMinutes(10))
    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
    .CacheIf(data => data.IsValid)
    .SetAsync("myKey", myData);
```

### Retrieving Data

```csharp
var result = await typedCache.GetAsync("myKey");
```

## License

This library is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.
