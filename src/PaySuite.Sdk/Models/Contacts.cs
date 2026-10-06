namespace PaySuite.Sdk.Models;

public sealed record CreateContactRequest
{
    public required string Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }   // E.164: +258841234567
}

public sealed record UpdateContactRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}

public sealed record Contact
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}