namespace PaySuite.Sdk;

/// <summary>
/// Opções para configuração do SDK PaySuite.
/// </summary>
public sealed class PaySuiteOptions
{
    public string Token { get; set; } = "";
    // A barra final é importante para combinar com caminhos relativos
    public Uri BaseUrl { get; set; } = new("https://paysuite.tech/api/v1/");
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}