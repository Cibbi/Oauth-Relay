using System.Text.Json.Serialization;

namespace OauthRelay;

/// <summary>
/// Represents an error response.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Represents an error response.
    /// </summary>
    public string Error { get; set; }
}


/// <summary>
/// Provides a JSON serializer context for the <see cref="JsonErrorResponseContext"/> class.
/// </summary>
[JsonSerializable(typeof(ErrorResponse))]
internal partial class JsonErrorResponseContext : JsonSerializerContext
{

}