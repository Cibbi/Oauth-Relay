using System.Text.Json.Serialization;

namespace OauthRelay;

public class VerifyRequest
{
    public string Code { get; set; } = "";
    public string? ExternalApiKey { get; set; }
    public string? ExternalKeyPosition { get; set; }
    public string? ExternalKeyName { get; set; }
}

[JsonSerializable(typeof(VerifyRequest))]
internal partial class JsonVerifyRequestContext : JsonSerializerContext
{

}