using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaySuite.Sdk.Models;

namespace PaySuite.Sdk.Internal;

internal sealed class ApiClient
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    public async Task<T> SendAsync<T>(HttpMethod method, string path,
        object? body = null, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new PaySuiteException($"Erro de rede: {ex.Message}", inner: ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new PaySuiteException("Tempo de espera esgotado.", inner: ex);
        }

        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new PaySuiteException(
                    ExtractMessage(raw) ?? "Ocorreu um erro desconhecido.",
                    (int)response.StatusCode, raw);

            if (string.IsNullOrWhiteSpace(raw))
                throw new PaySuiteException("Resposta vazia do servidor.", (int)response.StatusCode);

            try
            {
                return JsonSerializer.Deserialize<T>(raw, Json)
                    ?? throw new PaySuiteException("Resposta inválida do servidor.", (int)response.StatusCode, raw);
            }
            catch (JsonException ex)
            {
                throw new PaySuiteException("Não foi possível ler a resposta da API.",
                    (int)response.StatusCode, raw, ex);
            }
        }
    }

    /// <summary>Chama a API e devolve só o campo "data".</summary>
    public async Task<T> GetDataAsync<T>(HttpMethod method, string path,
        object? body = null, CancellationToken ct = default)
    {
        var r = await SendAsync<ApiResponse<T>>(method, path, body, ct).ConfigureAwait(false);
        return r.Data ?? throw new PaySuiteException("A resposta não contém 'data'.");
    }

    private static string? ExtractMessage(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
        }
        catch { return null; }
    }
}