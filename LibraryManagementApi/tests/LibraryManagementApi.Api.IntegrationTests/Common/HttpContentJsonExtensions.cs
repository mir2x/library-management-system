using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryManagementApi.Api.IntegrationTests.Common;

public static class HttpContentJsonExtensions
{
    // The API serializes enums as strings (see Program.cs's ConfigureHttpJsonOptions), but that
    // only configures the server's own Minimal API pipeline — a test's HttpClient has no idea
    // about it, so ReadFromJsonAsync<T>() without this would throw deserializing any DTO with
    // an enum property (LoanDto.Status, ReservationDto.Status, MemberDto.Status).
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static Task<T?> ReadFromJsonWithEnumsAsync<T>(this HttpContent content, CancellationToken cancellationToken = default) =>
        content.ReadFromJsonAsync<T>(Options, cancellationToken);

    public static Task<T?> GetFromJsonWithEnumsAsync<T>(this HttpClient client, string requestUri, CancellationToken cancellationToken = default) =>
        client.GetFromJsonAsync<T>(requestUri, Options, cancellationToken);
}
