namespace PaySuite.Sdk.Models;

public static class PaymentMethods
{
    public const string CreditCard = "credit_card";
    public const string MPesa = "mpesa";
    public const string Emola = "emola";
}

public sealed record CreatePaymentRequest
{
    public required decimal Amount { get; init; }          // em MZN
    public required string Reference { get; init; }        // único, máx. 50
    public string? Description { get; init; }              // máx. 125
    public string? Method { get; init; }
    public string? ReturnUrl { get; init; }
    public string? WebhookUrl { get; init; }
    public string? ContactId { get; init; }
}

public sealed record Payment
{
    public string Id { get; init; } = "";
    public decimal Amount { get; init; }
    public string Reference { get; init; } = "";
    public string Status { get; init; } = "";
    public string? Description { get; init; }
    public string? Method { get; init; }
    public string? ContactId { get; init; }
    public string? CheckoutUrl { get; init; }
    public PaymentTransaction? Transaction { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

public sealed record PaymentTransaction
{
    public long? Id { get; init; }
    public string? Status { get; init; }
    public string? TransactionId { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
}