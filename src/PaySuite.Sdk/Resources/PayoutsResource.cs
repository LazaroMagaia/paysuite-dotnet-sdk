using PaySuite.Sdk.Internal;
using PaySuite.Sdk.Models;

namespace PaySuite.Sdk.Resources;

public sealed class PayoutsResource
{
    private readonly ApiClient _api;

    internal PayoutsResource(ApiClient api) => _api = api;

    public Task<Payout> CreateAsync(
        CreatePayoutRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // -------------------------------------------------------------
        // Amount
        // PaySuite: 1 MZN até 1.000.000 MZN
        // -------------------------------------------------------------
        Guard.Range(
            request.Amount,
            1m,
            1_000_000m,
            nameof(request.Amount));

        // -------------------------------------------------------------
        // Reference (alfanumérica, máx. 30)
        // -------------------------------------------------------------
        Guard.NotEmpty(
            request.Reference,
            nameof(request.Reference));

        Guard.MaxLength(
            request.Reference,
            30,
            nameof(request.Reference));

        if (!request.Reference.All(IsAsciiLetterOrDigit))
        {
            throw new PaySuiteValidationException(
                "Reference deve ser alfanumérica (apenas letras A-Z e dígitos 0-9).");
        }

        // -------------------------------------------------------------
        // Description (opcional, máx. 255)
        // -------------------------------------------------------------
        Guard.MaxLength(
            request.Description,
            255,
            nameof(request.Description));

        // -------------------------------------------------------------
        // Currency
        // -------------------------------------------------------------
        Guard.NotEmpty(
            request.Currency,
            nameof(request.Currency));

        if (!string.Equals(
                request.Currency.Trim(),
                "MZN",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new PaySuiteValidationException(
                "A moeda do payout deve ser MZN.");
        }

        // -------------------------------------------------------------
        // Method
        // -------------------------------------------------------------
        Guard.NotEmpty(
            request.Method,
            nameof(request.Method));

        var method = request.Method.Trim().ToLowerInvariant();

        if (!PayoutMethods.IsValid(method))
        {
            throw new PaySuiteValidationException(
                $"Método de payout inválido: {request.Method}");
        }

        // -------------------------------------------------------------
        // Beneficiary
        // -------------------------------------------------------------
        ArgumentNullException.ThrowIfNull(request.Beneficiary);

        var beneficiary = request.Beneficiary with
        {
            Phone = request.Beneficiary.Phone?.Trim(),
            Nib = request.Beneficiary.Nib?.Trim(),
            Holder = request.Beneficiary.Holder?.Trim()
        };

        ValidateBeneficiary(method, beneficiary);

        // -------------------------------------------------------------
        // Webhook (opcional: se omitido, a PaySuite usa o da conta)
        // -------------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(request.WebhookUrl))
        {
            Guard.Url(
                request.WebhookUrl,
                nameof(request.WebhookUrl));
        }

        // Envia os valores normalizados, não os originais.
        var payload = request with
        {
            Method = method,
            Currency = "MZN",
            Beneficiary = beneficiary
        };

        return _api.GetDataAsync<Payout>(
            HttpMethod.Post,
            "payouts",
            payload,
            ct);
    }

    public Task<Payout> GetAsync(
        string id,
        CancellationToken ct = default)
        => _api.GetDataAsync<Payout>(
            HttpMethod.Get,
            $"payouts/{Guard.Id(id)}",
            ct: ct);

    public Task<PagedResponse<Payout>> ListAsync(
        int page = 1,
        int limit = 15,
        CancellationToken ct = default)
    {
        Guard.Range(
            page,
            1,
            int.MaxValue,
            nameof(page));

        // A doc não documenta um máximo para payouts; a API responde 422 se exceder.
        Guard.Range(
            limit,
            1,
            int.MaxValue,
            nameof(limit));

        return _api.SendAsync<PagedResponse<Payout>>(
            HttpMethod.Get,
            $"payouts?page={page}&limit={limit}",
            ct: ct);
    }

    private static void ValidateBeneficiary(
        string method,
        Beneficiary beneficiary)
    {
        switch (method)
        {
            case PayoutMethods.MPesa:
            case PayoutMethods.Emola:
            case PayoutMethods.Mkesh:
                ValidateMobileBeneficiary(beneficiary);
                break;

            case PayoutMethods.Bank:
            case PayoutMethods.BankTransfer:
                ValidateBankBeneficiary(beneficiary);
                break;

            default:
                throw new PaySuiteValidationException(
                    $"Sem validação de beneficiário para o método: {method}");
        }
    }

    private static void ValidateMobileBeneficiary(
        Beneficiary beneficiary)
    {
        if (string.IsNullOrWhiteSpace(beneficiary.Phone))
        {
            throw new PaySuiteValidationException(
                "Beneficiary.Phone é obrigatório para payouts móveis.");
        }

        if (beneficiary.Phone.Length != 9 || !beneficiary.Phone.All(IsAsciiDigit))
        {
            throw new PaySuiteValidationException(
                "Beneficiary.Phone deve conter 9 dígitos.");
        }

        ValidateHolder(beneficiary);
    }

    private static void ValidateBankBeneficiary(
        Beneficiary beneficiary)
    {
        if (string.IsNullOrWhiteSpace(beneficiary.Nib))
        {
            throw new PaySuiteValidationException(
                "Beneficiary.Nib é obrigatório para payouts bancários.");
        }

        if (beneficiary.Nib.Length != 21 || !beneficiary.Nib.All(IsAsciiDigit))
        {
            throw new PaySuiteValidationException(
                "Beneficiary.Nib deve conter 21 dígitos.");
        }

        ValidateHolder(beneficiary);
    }

    private static void ValidateHolder(Beneficiary beneficiary)
    {
        if (string.IsNullOrWhiteSpace(beneficiary.Holder))
        {
            throw new PaySuiteValidationException(
                "Beneficiary.Holder (titular) é obrigatório.");
        }
    }

    // char.IsDigit aceita dígitos Unicode de outros alfabetos; a API quer só 0-9.
    private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

    private static bool IsAsciiLetterOrDigit(char c)
        => c is >= '0' and <= '9'
            or >= 'A' and <= 'Z'
            or >= 'a' and <= 'z';
}