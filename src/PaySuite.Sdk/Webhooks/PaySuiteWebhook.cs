using System.Text;
using System.Text.Json;

namespace PaySuite.Sdk.Webhooks;

public static class PaySuiteWebhook
{
    /// <summary>Lê o corpo, valida a assinatura e interpreta o evento.</summary>
    public static async Task<WebhookResult> ProcessAsync(
        Stream body, string? signature, string secret, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(body);

        using var reader = new StreamReader(body, Encoding.UTF8, false, 1024, leaveOpen: true);
        return Process(await reader.ReadToEndAsync(ct), signature, secret);
    }

    /// <summary>
    /// rawBody tem de ser exatamente o texto recebido: a assinatura é calculada sobre ele.
    /// </summary>
    public static WebhookResult Process(string rawBody, string? signature, string secret)
    {
        ArgumentNullException.ThrowIfNull(rawBody);

        // Erro de configuração, não um pedido inválido.
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("O segredo do webhook não pode estar vazio.", nameof(secret));

        if (string.IsNullOrWhiteSpace(signature))
            return Fail(WebhookStatus.MissingSignature, "Cabeçalho X-Signature em falta.");

        if (!WebhookValidator.IsValid(rawBody, signature, secret.Trim()))
            return Fail(WebhookStatus.InvalidSignature, "Assinatura inválida.");

        WebhookEvent evt;
        try
        {
            evt = WebhookValidator.Parse(rawBody);
        }
        catch (Exception ex) when (ex is JsonException or PaySuiteException)
        {
            return Fail(WebhookStatus.InvalidPayload, "Payload inválido.");
        }

        if (string.IsNullOrWhiteSpace(evt.Event) || evt.Data is null ||
            string.IsNullOrWhiteSpace(evt.Data.Id) || string.IsNullOrWhiteSpace(evt.Data.Reference))
            return Fail(WebhookStatus.MissingData, "Dados do evento em falta.", evt);

        return new WebhookResult
        {
            Status = WebhookStatus.Valid,
            Type = ToType(evt.Event),
            Event = evt
        };
    }

    private static WebhookResult Fail(WebhookStatus status, string error, WebhookEvent? evt = null)
        => new() { Status = status, Error = error, Event = evt };

    private static WebhookEventType ToType(string name) => name switch
    {
        WebhookEvents.PaymentSuccess => WebhookEventType.PaymentSuccess,
        WebhookEvents.PaymentFailed => WebhookEventType.PaymentFailed,
        WebhookEvents.PayoutSuccess => WebhookEventType.PayoutSuccess,
        WebhookEvents.PayoutFailed => WebhookEventType.PayoutFailed,
        WebhookEvents.RefundSuccess => WebhookEventType.RefundSuccess,
        WebhookEvents.RefundFailed => WebhookEventType.RefundFailed,
        _ => WebhookEventType.Unknown
    };
}