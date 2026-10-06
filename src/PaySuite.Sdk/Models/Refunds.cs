namespace PaySuite.Sdk.Models;

public sealed record CreateRefundRequest
{
    public required string PaymentId { get; init; }
    public required decimal Amount { get; init; }   // 0.01 a 10.000.000
    public required string Reason { get; init; }    // máx. 500
    public string? WebhookUrl { get; init; }
}

public sealed record Refund
{
    public string Id { get; init; } = "";
    public string PaymentId { get; init; } = "";
    public string Reference { get; init; } = "";
    public decimal Amount { get; init; }
    public string Status { get; init; } = "";   // pending, processing, completed, failed, cancelled
    public string? Reason { get; init; }
}