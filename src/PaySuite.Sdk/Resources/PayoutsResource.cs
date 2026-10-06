using PaySuite.Sdk.Internal;
using PaySuite.Sdk.Models;

namespace PaySuite.Sdk.Resources;

public sealed class PayoutsResource
{
    private readonly ApiClient _api;
    internal PayoutsResource(ApiClient api) => _api = api;

    public Task<Payout> CreateAsync(CreatePayoutRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guard.Positive(request.Amount, nameof(request.Amount));
        Guard.NotEmpty(request.Reference, nameof(request.Reference));
        Guard.MaxLength(request.Reference, 30, nameof(request.Reference));
        Guard.MaxLength(request.Description, 255, nameof(request.Description));
        Guard.NotEmpty(request.Method, nameof(request.Method));
        Guard.Url(request.WebhookUrl, nameof(request.WebhookUrl));
        return _api.GetDataAsync<Payout>(HttpMethod.Post, "payouts", request, ct);
    }

    public Task<Payout> GetAsync(string id, CancellationToken ct = default)
        => _api.GetDataAsync<Payout>(HttpMethod.Get, $"payouts/{Guard.Id(id)}", ct: ct);

    public Task<PagedResponse<Payout>> ListAsync(int page = 1, int limit = 15, CancellationToken ct = default)
        => _api.SendAsync<PagedResponse<Payout>>(HttpMethod.Get, $"payouts?page={page}&limit={limit}", ct: ct);
}