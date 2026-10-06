namespace PaySuite.Sdk;

public class PaySuiteException : Exception
{
    public int? StatusCode { get; }
    public string? ResponseBody { get; }

    public PaySuiteException(string message, int? statusCode = null,
        string? responseBody = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

/// <summary>Erro de validação local, antes de chamar a API.</summary>
public sealed class PaySuiteValidationException : PaySuiteException
{
    public PaySuiteValidationException(string message) : base(message) { }
}