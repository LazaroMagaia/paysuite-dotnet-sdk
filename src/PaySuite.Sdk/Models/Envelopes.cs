using System.Text.Json;
namespace PaySuite.Sdk.Models;

public sealed class ApiResponse<T>
{
    public string Status { get; init; } = "";
    public T? Data { get; init; }
    public string? Message { get; init; }
}

public sealed class PagedResponse<T>
{
    public string Status { get; init; } = "";
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
    public JsonElement? Links { get; init; }
    public JsonElement? Meta { get; init; }
}