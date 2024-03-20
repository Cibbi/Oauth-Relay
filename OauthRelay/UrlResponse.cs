using System.Text.Json.Serialization;

namespace OauthRelay;

/// <summary>
/// Represents the response from a URL.
/// </summary>
public class UrlResponse
{
    /// <summary>
    /// Represents a response object with a URL property.
    /// </summary>
    public string Url { get; set; }
}

/// <summary>
/// Provides JSON serialization and deserialization for the <see cref="JsonUrlResponseContext.UrlResponse"/> class.
/// </summary>
[JsonSerializable(typeof(UrlResponse))]
internal partial class JsonUrlResponseContext : JsonSerializerContext
{

}