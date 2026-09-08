using System.Net;
using System.Text.Json;
using System.Globalization;

namespace DirectoryService.IntegrationTests.Infrastructure;

internal static class HttpResponseAssertions
{
    public static async Task<Guid> AssertCreatedGuidAsync(this HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await ReadJsonAsync(response);
        AssertEnvelopeShape(document.RootElement, isError: false);

        var result = document.RootElement.GetProperty("result").GetGuid();
        Assert.NotEqual(Guid.Empty, result);
        return result;
    }

    public static async Task AssertSuccessAsync(
        this HttpResponseMessage response,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Assert.Equal(statusCode, response.StatusCode);
        using var document = await ReadJsonAsync(response);
        AssertEnvelopeShape(document.RootElement, isError: false);
        Assert.True(document.RootElement.GetProperty("result").ValueKind is JsonValueKind.Null or JsonValueKind.Object or JsonValueKind.Array);
    }

    public static async Task AssertErrorAsync(
        this HttpResponseMessage response,
        HttpStatusCode statusCode,
        string errorCode)
    {
        Assert.Equal(statusCode, response.StatusCode);
        using var document = await ReadJsonAsync(response);
        var root = document.RootElement;
        AssertEnvelopeShape(root, isError: true);

        var error = root.GetProperty("error");
        var expectedErrorType = statusCode switch
        {
            HttpStatusCode.BadRequest => "Validation",
            HttpStatusCode.NotFound => "NotFound",
            HttpStatusCode.Conflict => "Conflict",
            _ => throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Для этого HTTP-статуса не определён тип ошибки.")
        };
        Assert.Equal(expectedErrorType, error.GetProperty("type").GetString());
        var firstMessage = error.GetProperty("messages")[0];
        Assert.Equal(errorCode, firstMessage.GetProperty("code").GetString());
    }

    public static async Task<JsonDocument> ReadJsonDocumentAsync(this HttpResponseMessage response)
    {
        return await ReadJsonAsync(response);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    private static void AssertEnvelopeShape(JsonElement root, bool isError)
    {
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal(isError, root.GetProperty("isError").GetBoolean());
        Assert.True(root.TryGetProperty("timeGenerated", out var timeGenerated));
        Assert.True(DateTime.TryParse(
            timeGenerated.GetString(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var generatedAt));
        Assert.NotEqual(default, generatedAt);

        if (isError)
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("result").ValueKind);
            Assert.NotEqual(JsonValueKind.Null, root.GetProperty("error").ValueKind);
        }
        else
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("error").ValueKind);
        }
    }
}
