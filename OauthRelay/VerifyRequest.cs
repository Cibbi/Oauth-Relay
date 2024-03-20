using System.Text.Json.Serialization;

namespace OauthRelay;

public class VerifyRequest
{
    public string Code { get; set; } = "";
    public string? ExternalApiKey { get; set; }
}

[JsonSerializable(typeof(VerifyRequest))]
internal partial class JsonVerifyRequestContext : JsonSerializerContext
{

}