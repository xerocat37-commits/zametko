using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ZAMETKI.Api;

public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public string? Token { get; set; }

    public Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);

    public Task<T> PostAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Post, path, body);

    public Task PostAsync(string path, object? body) => SendAsync<object?>(HttpMethod.Post, path, body, expectBody: false);

    public Task<T> PutAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Put, path, body);

    public Task DeleteAsync(string path) => SendAsync<object?>(HttpMethod.Delete, path, null, expectBody: false);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, bool expectBody = true)
    {
        using var req = new HttpRequestMessage(method, path);
        if (!string.IsNullOrEmpty(Token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

        if (body is not null)
            req.Content = JsonContent.Create(body, options: JsonOptions);

        using var resp = await _http.SendAsync(req).ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            var errorText = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new ApiException(resp.StatusCode, ExtractError(errorText) ?? resp.ReasonPhrase ?? "API error");
        }

        if (!expectBody || resp.StatusCode == HttpStatusCode.NoContent)
            return default!;

        var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions).ConfigureAwait(false);
        return result!;
    }

    private static string? ExtractError(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err)) return err.GetString();
            if (doc.RootElement.TryGetProperty("title", out var title)) return title.GetString();
        }
        catch (JsonException) { }
        return body.Length > 200 ? body[..200] : body;
    }
}
