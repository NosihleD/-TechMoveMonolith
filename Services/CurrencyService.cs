using System.Text.Json;

namespace TechMove.Services;

public class CurrencyService
{
    public const decimal FallbackRate = 18.50m;

    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("USD", cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var data = await JsonSerializer.DeserializeAsync<ExchangeRateResponse>(stream, cancellationToken: cancellationToken);

            if (data?.Rates is null || !data.Rates.TryGetValue("ZAR", out var rate) || rate <= 0)
            {
                _logger.LogWarning("USD to ZAR exchange rate was not available from the upstream API.");
                return FallbackRate;
            }

            return rate;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Currency API unavailable. Using fallback USD -> ZAR rate.");
            return FallbackRate;
        }
    }

    public static decimal ConvertUsdToZar(decimal usd, decimal rate) =>
        Math.Round(usd * rate, 2, MidpointRounding.AwayFromZero);

    private sealed class ExchangeRateResponse
    {
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
