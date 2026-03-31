using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class AmadeusTokenService(HttpClient httpClient, IOptions<AmadeusSettings> settings, ILogger<AmadeusTokenService> logger)
{
    private readonly AmadeusSettings _settings = settings.Value;
    private string? _accessToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_accessToken is not null && DateTimeOffset.UtcNow < _expiresAt)
            return _accessToken;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (_accessToken is not null && DateTimeOffset.UtcNow < _expiresAt)
                return _accessToken;

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret
            });

            var response = await httpClient.PostAsync($"{_settings.BaseUrl}/v1/security/oauth2/token", content, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            _accessToken = json.GetProperty("access_token").GetString()!;
            var expiresIn = json.GetProperty("expires_in").GetInt32();

            // Expire 60s early to avoid edge cases
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 60);

            logger.LogInformation("Amadeus OAuth2 token acquired, expires in {ExpiresIn}s", expiresIn);
            return _accessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to acquire Amadeus OAuth2 token");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}
