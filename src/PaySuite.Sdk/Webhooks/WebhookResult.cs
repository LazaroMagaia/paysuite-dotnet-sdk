namespace PaySuite.Sdk.Webhooks;

public enum WebhookStatus
{
    Valid,
    MissingSignature,
    InvalidSignature,
    InvalidPayload,
    MissingData
}

public enum WebhookEventType
{
    Unknown,
    PaymentSuccess,
    PaymentFailed,
    PayoutSuccess,
    PayoutFailed,
    RefundSuccess,
    RefundFailed
}

public sealed record WebhookResult
{
    public WebhookStatus Status { get; init; }
    public WebhookEventType Type { get; init; }
    public WebhookEvent? Event { get; init; }
    public string? Error { get; init; }

    public bool IsValid => Status == WebhookStatus.Valid;

    /// <summary>Código HTTP sugerido para responder à PaySuite.</summary>
    public int StatusCode => Status switch
    {
        WebhookStatus.Valid => 200,
        WebhookStatus.InvalidPayload => 400,
        WebhookStatus.MissingData => 422,
        _ => 401
    };
}