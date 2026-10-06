using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PaySuite.Sdk.Webhooks;

public static class WebhookEvents
{
    public const string PaymentSuccess = "payment.success";
    public const string PaymentFailed = "payment.failed";
    public const string PayoutSuccess = "payout.success";
    public const string PayoutFailed = "payout.failed";
    public const string RefundSuccess = "refund.success";
    public const string RefundFailed = "refund.failed";
}

public sealed record WebhookData
{
    public string Id { get; init; } = "";
    public decimal Amount { get; init; }
    public string Reference { get; init; } = "";
    public string? Method { get; init; }
    public string? Status { get; init; }
    public string? PaymentId { get; init; }   // só em eventos refund.*
}

public sealed record WebhookEvent
{
    public string Event { get; init; } = "";
    public WebhookData Data { get; init; } = new();
}

public static class WebhookValidator
{
    public static bool IsValid(string rawBody, string? signature, string secret)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrEmpty(secret)) return false;

        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(rawBody));
        var expected = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected),
            Encoding.ASCII.GetBytes(signature.Trim().ToLowerInvariant()));
    }

    public static WebhookEvent Parse(string rawBody)
        => JsonSerializer.Deserialize<WebhookEvent>(rawBody, Internal.ApiClient.Json)
           ?? throw new PaySuiteException("Payload de webhook inválido.");
}