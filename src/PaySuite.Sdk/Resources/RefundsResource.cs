using PaySuite.Sdk.Internal;
using PaySuite.Sdk.Models;

namespace PaySuite.Sdk.Resources;

public sealed class RefundsResource
{
    private readonly ApiClient _api;
    internal RefundsResource(ApiClient api) => _api = api;

    public Task<Refund> CreateAsync(CreateRefundRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guard.NotEmpty(request.PaymentId, nameof(request.PaymentId));
        Guard.Positive(request.Amount, nameof(request.Amount));
        Guard.NotEmpty(request.Reason, nameof(request.Reason));
        Guard.MaxLength(request.Reason, 500, nameof(request.Reason));
        Guard.Url(request.WebhookUrl, nameof(request.WebhookUrl));
        return _api.GetDataAsync<Refund>(HttpMethod.Post, "refunds", request, ct);
    }

    public Task<Refund> GetAsync(string id, CancellationToken ct = default)
        => _api.GetDataAsync<Refund>(HttpMethod.Get, $"refunds/{Guard.Id(id)}", ct: ct);

    public Task<PagedResponse<Refund>> ListAsync(int page = 1, int limit = 20, CancellationToken ct = default)
        => _api.SendAsync<PagedResponse<Refund>>(HttpMethod.Get, $"refunds?page={page}&limit={limit}", ct: ct);
}