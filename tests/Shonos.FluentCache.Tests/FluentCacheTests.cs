using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;

namespace Shonos.FluentCache.Tests
{
    public class FluentCacheTests
    {
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly FluentCache _fluentCache;

        public FluentCacheTests()
        {
            _mockCache = new Mock<IDistributedCache>();
            _fluentCache = new FluentCache(_mockCache.Object);
        }

        [Fact]
        public void CreateWithOnCacheMiss_ShouldReturnFluentCacheOfT()
        {
            // Arrange
            Func<Task<int>> action = () => Task.FromResult(42);

            // Act
            var result = _fluentCache.CreateWithOnCacheMiss(action);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FluentCache<int>>(result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldCallRemoveAsyncOnCache()
        {
            // Arrange
            var key = "testKey";

            // Act
            await _fluentCache.RemoveAsync(key);

            // Assert
            _mockCache.Verify(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_CacheMiss_ReturnsValueFromOnCacheMiss()
        {
            // Arrange
            var cacheKey = "test_key";

            var expectedObjectValue = new SampleCachedObject()
            {
                Value1 = "testvalue",
            };

            // Setup OnCacheMiss function
            Func<Task<SampleCachedObject>> onCacheMiss = () => Task.FromResult(expectedObjectValue);

            var fluentCache = new FluentCache(_mockCache.Object);
            var typedFluentCache = fluentCache.CreateWithOnCacheMiss(onCacheMiss);
            // Act
            var result = await typedFluentCache.GetAsync(cacheKey);

            // Assert
            result.Should().BeEquivalentTo(expectedObjectValue);

            // Verify that IDisitributedCache called Set Async
            _mockCache.Verify(c => c.SetAsync(cacheKey, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_CacheHit_ReturnsValueFromCache()
        {
            // Arrange
            var cacheKey = "test_key";

            var expectedObjectValue = new SampleCachedObject()
            {
                Value1 = "testvalue",
            };

            // Setup OnCacheMiss function
            Func<Task<SampleCachedObject>> onCacheMiss = () => Task.FromResult(expectedObjectValue);

            var fluentCache = new FluentCache(_mockCache.Object);
            var typedFluentCache = fluentCache.CreateWithOnCacheMiss(onCacheMiss);

            // Simulate cache hit
            string expectedObjectValueAsJson = JsonSerializer.Serialize(expectedObjectValue);
            var expectedObjectValueAsBytes = Encoding.UTF8.GetBytes(expectedObjectValueAsJson);
            _mockCache.Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>())).ReturnsAsync(expectedObjectValueAsBytes);

            // Act
            var result = await typedFluentCache.GetAsync(cacheKey);

            // Assert
            result.Should().BeEquivalentTo(expectedObjectValue);

            // Verify that IDistributedCache never called SetAsync
            _mockCache.Verify(c => c.SetAsync(cacheKey, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        protected class SampleCachedObject
        {
            public string Value1 { get; set; }
            public string? Value2 { get; set; }
        }
    }
}
