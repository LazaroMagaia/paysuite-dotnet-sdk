namespace PaySuite.Sdk.Internal;

internal static class Guard
{
    public static void NotEmpty(string? v, string name)
    {
        if (string.IsNullOrWhiteSpace(v))
            throw new PaySuiteValidationException($"Campo obrigatório em falta: {name}");
    }

    public static void MaxLength(string? v, int max, string name)
    {
        if (v is not null && v.Length > max)
            throw new PaySuiteValidationException($"{name} não pode ter mais de {max} caracteres.");
    }

    public static void Url(string? v, string name)
    {
        if (v is null) return;
        if (!Uri.TryCreate(v, UriKind.Absolute, out var u) ||
            (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps))
            throw new PaySuiteValidationException($"URL inválido em {name}.");
    }

    public static void Positive(decimal v, string name)
    {
        if (v <= 0) throw new PaySuiteValidationException($"{name} deve ser um número positivo.");
    }

    /// <summary>Valida e escapa um ID para uso no caminho do URL.</summary>
    public static string Id(string? id, string name = "id")
    {
        NotEmpty(id, name);
        return Uri.EscapeDataString(id!.Trim());
    }
}