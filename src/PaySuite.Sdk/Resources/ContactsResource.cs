using PaySuite.Sdk.Internal;
using PaySuite.Sdk.Models;

namespace PaySuite.Sdk.Resources;

public sealed class ContactsResource
{
    private readonly ApiClient _api;

    internal ContactsResource(ApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Cria um novo contacto.
    /// </summary>
    public Task<ApiResponse<Contact>> CreateAsync(
        CreateContactRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Guard.NotEmpty(
            request.Name,
            nameof(request.Name));

        if (string.IsNullOrWhiteSpace(request.Email) &&
            string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new PaySuiteValidationException(
                "Indica pelo menos o email ou o telefone.");
        }

        return _api.GetDataAsync<Contact>(
            HttpMethod.Post,
            "contacts",
            request,
            ct);
    }

    /// <summary>
    /// Obtém os detalhes de um contacto.
    /// </summary>
    public Task<ApiResponse<Contact>> GetAsync(
        string id,
        CancellationToken ct = default)
    {
        return _api.GetDataAsync<Contact>(
            HttpMethod.Get,
            $"contacts/{Guard.Id(id)}",
            ct: ct);
    }

    /// <summary>
    /// Atualiza um contacto.
    /// </summary>
    public Task<ApiResponse<Contact>> UpdateAsync(
        string id,
        UpdateContactRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _api.GetDataAsync<Contact>(
            HttpMethod.Patch,
            $"contacts/{Guard.Id(id)}",
            request,
            ct);
    }

    /// <summary>
    /// Remove um contacto.
    /// </summary>
    public Task<ApiResponse<object>> DeleteAsync(
        string id,
        CancellationToken ct = default)
    {
        return _api.SendAsync<ApiResponse<object>>(
            HttpMethod.Delete,
            $"contacts/{Guard.Id(id)}",
            ct: ct);
    }

    /// <summary>
    /// Obtém a lista paginada de contactos.
    /// </summary>
    public Task<PagedResponse<Contact>> ListAsync(
        int page = 1,
        int limit = 20,
        string? email = null,
        CancellationToken ct = default)
    {
        Guard.Range(
            page,
            1,
            int.MaxValue,
            nameof(page));

        Guard.Range(
            limit,
            1,
            100,
            nameof(limit));

        var path = $"contacts?page={page}&limit={limit}";

        if (!string.IsNullOrWhiteSpace(email))
        {
            path += $"&email={Uri.EscapeDataString(email)}";
        }

        return _api.SendAsync<PagedResponse<Contact>>(
            HttpMethod.Get,
            path,
            ct: ct);
    }
}