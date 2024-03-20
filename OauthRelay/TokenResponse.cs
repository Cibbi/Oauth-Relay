using System.Text.Json.Serialization;

namespace OauthRelay;

/// <summary>
/// Represents a token response containing access token, token type, and scope.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Represents an access token.
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Represents the type of a token.
    /// </summary>
    public string TokenType { get; set; }

    /// <summary>
    /// Represents the scope property of a token response.
    /// </summary>
    public string Scope { get; set; }
}

/// <summary>
/// Represents the context for serializing and deserializing objects of type TokenResponse using the JSON format.
/// </summary>
[JsonSerializable(typeof(TokenResponse))]
internal partial class JsonTokenResponseContext : JsonSerializerContext
{

}