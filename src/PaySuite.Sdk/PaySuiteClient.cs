using System.Net.Http.Headers;
using PaySuite.Sdk.Internal;
using PaySuite.Sdk.Resources;

namespace PaySuite.Sdk;

public sealed class PaySuiteClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;

    public PaymentsResource Payments { get; }
    public PayoutsResource Payouts { get; }
    public RefundsResource Refunds { get; }
    public ContactsResource Contacts { get; }

    /// <summary>Uso simples: só com o token.</summary>
    public PaySuiteClient(string token, PaySuiteOptions? options = null)
        : this(CreateHttpClient(token, options), ownsHttpClient: true) { }

    /// <summary>Uso com DI / IHttpClientFactory. O HttpClient já deve estar configurado.</summary>
    public PaySuiteClient(HttpClient httpClient) : this(httpClient, ownsHttpClient: false) { }

    private PaySuiteClient(HttpClient http, bool ownsHttpClient)
    {
        _http = http;
        _ownsHttpClient = ownsHttpClient;
        var api = new ApiClient(http);
        Payments = new PaymentsResource(api);
        Payouts = new PayoutsResource(api);
        Refunds = new RefundsResource(api);
        Contacts = new ContactsResource(api);
    }

    public static void Configure(HttpClient http, PaySuiteOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Token))
            throw new ArgumentException("O token não pode estar vazio.", nameof(options));

        http.BaseAddress = options.BaseUrl;
        http.Timeout = options.Timeout;
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Token.Trim());
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        http.DefaultRequestHeaders.UserAgent.ParseAdd("PaySuite-DotNet/1.0");
    }

    private static HttpClient CreateHttpClient(string token, PaySuiteOptions? options)
    {
        var o = options ?? new PaySuiteOptions();
        o.Token = token;
        var http = new HttpClient();
        Configure(http, o);
        return http;
    }

    public void Dispose()
    {
        if (_ownsHttpClient) _http.Dispose();
    }
}