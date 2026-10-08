using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using TechMove.Services;

namespace TechMove.Tests;

public class CurrencyServiceTests
{
    [Fact]
    public void ConvertUsdToZar_UsesProvidedRate()
    {
        var result = CurrencyService.ConvertUsdToZar(100m, 18.50m);
        Assert.Equal(1850.00m, result);
    }

    [Theory]
    [InlineData(10, 18.50, 185.00)]
    [InlineData(25.55, 18.50, 472.68)]
    [InlineData(99.99, 18.00, 1799.82)]
    public void ConvertUsdToZar_RoundsToTwoDecimals(decimal usd, decimal rate, decimal expected)
    {
        Assert.Equal(expected, CurrencyService.ConvertUsdToZar(usd, rate));
    }

    [Fact]
    public async Task GetUsdToZarRateAsync_UsesFallbackWhenApiIsUnavailable()
    {
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var service = new CurrencyService(httpClient, NullLogger<CurrencyService>.Instance);

        var result = await service.GetUsdToZarRateAsync();

        Assert.Equal(CurrencyService.FallbackRate, result);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }
}
