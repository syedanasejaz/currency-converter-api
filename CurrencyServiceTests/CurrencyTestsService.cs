using CurrencyConverterApi.Services.Implementation;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyServiceTests
{
    public class CurrencyTestsService
    {
        private readonly CurrencyService _currencyService;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IMemoryCache> _mockMemoryCache;

        public CurrencyTestsService()
        {
            // Mock HttpMessageHandler
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Mock IMemoryCache
            _mockMemoryCache = new Mock<IMemoryCache>();

            // Create HttpClient with the mocked HttpMessageHandler
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            // Mock the TryGetValue method of IMemoryCache to simulate cache hit
            _mockMemoryCache
                .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns((object key, out object value) =>
                {
                    value = new Dictionary<string, decimal> { { "USD", 1.1m }, { "GBP", 0.85m } }; // Simulated cached data
                    return true; // Simulate cache hit
                });

            // Mock the Set method of IMemoryCache
            _mockMemoryCache
                .Setup(cache => cache.Set(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()))
                .Returns((object key, object value, MemoryCacheEntryOptions options) => value); // Just return the value to simulate cache set

            // Initialize CurrencyService with both HttpClient and IMemoryCache
            _currencyService = new CurrencyService(httpClient, _mockMemoryCache.Object);
        }


        [Fact]
        public async Task GetLatestRatesAsync_ShouldUseCache()
        {
            // Arrange: Fake API response
            var fakeResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"rates\": {\"USD\": 1.1, \"GBP\": 0.85}}")
            };

            // Mock SendAsync to return the fake response
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(fakeResponse);

            // Act: First request to the service
            var resultFirstCall = await _currencyService.GetLatestRatesAsync("EUR");

            // Assert: Ensure the result is as expected
            Assert.NotNull(resultFirstCall);
            Assert.True(resultFirstCall.ContainsKey("USD"));
            Assert.Equal(1.1m, resultFirstCall["USD"]);

            // Act: Second request to check cache usage
            var resultSecondCall = await _currencyService.GetLatestRatesAsync("EUR");

            // Assert: Ensure the second call doesn't hit the network (check if it's cached)
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(), // Ensures SendAsync is called only once (i.e., cache is used)
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
