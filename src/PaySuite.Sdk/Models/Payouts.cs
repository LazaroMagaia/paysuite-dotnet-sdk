namespace PaySuite.Sdk.Models;

public sealed record Beneficiary
{
    public string? Phone { get; init; }   // mpesa/emola/mkesh: 9 dígitos
    public string? Nib { get; init; }     // bank: 21 dígitos
    public string? Holder { get; init; }
}

public sealed record CreatePayoutRequest
{
    public required decimal Amount { get; init; }          // 1 a 1.000.000
    public string Currency { get; init; } = "MZN";
    public required string Reference { get; init; }        // alfanumérico, máx. 30
    public required string Method { get; init; }           // mpesa, emola, mkesh, bank, bank_transfer
    public required Beneficiary Beneficiary { get; init; }
    public string? Description { get; init; }              // máx. 255
    public string? WebhookUrl { get; init; }
}

public sealed record Payout
{
    public string Id { get; init; } = "";
    public decimal Amount { get; init; }
    public string Reference { get; init; } = "";
    public string Status { get; init; } = "";   // pending, completed, failed, cancelled
    public string? Description { get; init; }
    public string? Method { get; init; }
    public Beneficiary? Beneficiary { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}