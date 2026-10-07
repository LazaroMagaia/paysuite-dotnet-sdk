using PaySuite.Sdk.Internal;
using PaySuite.Sdk.Models;

namespace PaySuite.Sdk.Resources;

public sealed class PaymentsResource
{
    private readonly ApiClient _api;

    internal PaymentsResource(ApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Cria um novo pagamento.
    /// </summary>
    public Task<ApiResponse<Payment>> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Guard.Range(
            request.Amount,
            10m,
            1_000_000m,
            nameof(request.Amount));

        Guard.NotEmpty(
            request.Reference,
            nameof(request.Reference));

        Guard.MaxLength(
            request.Reference,
            50,
            nameof(request.Reference));

        Guard.MaxLength(
            request.Description,
            125,
            nameof(request.Description));

        // return_url e webhook_url são opcionais na API.
        if (!string.IsNullOrWhiteSpace(request.ReturnUrl))
        {
            Guard.Url(
                request.ReturnUrl,
                nameof(request.ReturnUrl));
        }

        if (!string.IsNullOrWhiteSpace(request.WebhookUrl))
        {
            Guard.Url(
                request.WebhookUrl,
                nameof(request.WebhookUrl));
        }

        return _api.GetDataAsync<Payment>(
            HttpMethod.Post,
            "payments",
            request,
            ct);
    }

    /// <summary>
    /// Obtém os detalhes de um pagamento específico.
    /// </summary>
    public Task<ApiResponse<Payment>> GetAsync(
        string id,
        CancellationToken ct = default)
    {
        return _api.GetDataAsync<Payment>(
            HttpMethod.Get,
            $"payments/{Guard.Id(id)}",
            ct: ct);
    }

    /// <summary>
    /// Obtém a lista paginada de pagamentos.
    /// </summary>
    public Task<ApiResponse<PagedResponse<Payment>>> ListAsync(
        int page = 1,
        int limit = 20,
        CancellationToken ct = default)
    {
        Guard.Range(
            page,
            1,
            int.MaxValue,
            nameof(page));

        Guard.Range(
            limit,
            1,
            100,
            nameof(limit));

        return _api.GetDataAsync<PagedResponse<Payment>>(
            HttpMethod.Get,
            $"payments?page={page}&limit={limit}",
            ct: ct);
    }
}