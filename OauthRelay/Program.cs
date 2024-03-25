using System.Net;
using System.Text.Json;
using OauthRelay;
using OauthRelay.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateSlimBuilder(args);

// Adding AOT support
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, JsonTokenResponseContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(1, JsonUrlResponseContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(2, JsonErrorResponseContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Insert(3, JsonVerifyRequestContext.Default);
});
builder.Services.AddHttpClient();

var app = builder.Build();

var oauthProvider = Environment.GetEnvironmentVariable("OAUTH_PROVIDER") switch
{
    "GITHUB" => OauthProvider.Github,
    _ => OauthProvider.Github
};

var oauthBaseUrl = Environment.GetEnvironmentVariable("OAUTH_BASE_URL") ?? "https://github.com";

if (oauthBaseUrl.EndsWith('/'))
    oauthBaseUrl = oauthBaseUrl[..^2];

var verifyEndpoint = Environment.GetEnvironmentVariable("VERIFY_ENDPOINT") ?? "";
var getAuthLinkEndpoint = Environment.GetEnvironmentVariable("AUTH_LINK_ENDPOINT") ?? "";
var clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";
var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "";

var externalVerificationServerUrl = Environment.GetEnvironmentVariable("EXTERNAL_VERIFICATION_URL") ?? "";
var externalVerificationServerMethod = Environment.GetEnvironmentVariable("EXTERNAL_VERIFICATION_URL") switch
{
    "GET" => HttpMethod.Get,
    "POST" => HttpMethod.Post,
    _ => null
};
var externalVerificationKeyPosition = Environment.GetEnvironmentVariable("EXTERNAL_VERIFICATION_KEY_POSITION") switch
{
    "HEADER" => KeyPosition.Header,
    "FORM" => KeyPosition.Form,
    "JSON" => KeyPosition.Json,
    _ => KeyPosition.Null,
};
var externalVerificationKeyName = Environment.GetEnvironmentVariable("EXTERNAL_VERIFICATION_KEY_NAME") ?? "";
if (!int.TryParse(Environment.GetEnvironmentVariable("EXTERNAL_VERIFICATION_RESPONSE_CODE"),
        out var externalVerificationResponseCode))
{
    externalVerificationResponseCode = -1;
}


app.MapPost(verifyEndpoint, async Task<Results<Ok<TokenResponse>, BadRequest<ErrorResponse>>> (HttpContext context) =>
{
    var data = await context.Request.ReadFromJsonAsync<VerifyRequest>();

    if (data is null)
    {
        return TypedResults.BadRequest(new ErrorResponse { Error = "Missing data" });
    }

    var code = data.Code;

    var externalKey = data.ExternalApiKey;

    var externalVerificationKeyNameOverride = data.ExternalKeyName;
    var externalVerificationKeyPositionOverride = data.ExternalKeyPosition switch
    {
        "HEADER" => KeyPosition.Header,
        "FORM" => KeyPosition.Form,
        "JSON" => KeyPosition.Json,
        _ => KeyPosition.Null,
    };

    var currentVerificationKeyName = string.IsNullOrWhiteSpace(externalVerificationKeyNameOverride)
        ? externalVerificationKeyName
        : externalVerificationKeyNameOverride;
    
    var currentVerificationKeyPosition = externalVerificationKeyPositionOverride != KeyPosition.Null
        ? externalVerificationKeyPositionOverride
        : externalVerificationKeyPosition;
    
    
    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
    var client = httpClientFactory.CreateClient();

    if (!string.IsNullOrEmpty(externalVerificationServerUrl) && externalVerificationServerMethod is not null &&
        currentVerificationKeyPosition != KeyPosition.Null &&
        !string.IsNullOrEmpty(currentVerificationKeyName) && externalVerificationResponseCode != -1)
    {
        if (string.IsNullOrWhiteSpace(externalKey))
        {
            return TypedResults.BadRequest(new ErrorResponse { Error = "Missing data" });
        }
        
        var externalRequest = new HttpRequestMessage(externalVerificationServerMethod, externalVerificationServerUrl)
        {
            Headers =
            {
                { "Accept", "application/json" }
            }
        };
        if (currentVerificationKeyPosition == KeyPosition.Header)
        {
            externalRequest.Headers.Add(currentVerificationKeyName, externalKey);
        }
        else if (currentVerificationKeyPosition == KeyPosition.Form)
        {
            externalRequest.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new(currentVerificationKeyName, externalKey)
            });
        }
        else
        {
            externalRequest.Content = 
                new StringContent($$"""
                                    {
                                        "{{currentVerificationKeyName}}": "{{externalKey}}"
                                    }
                                    """);
        }

        var externalResponse = await client.SendAsync(externalRequest);
        if (externalResponse.StatusCode != (HttpStatusCode) externalVerificationResponseCode)
        {
            return TypedResults.BadRequest(new ErrorResponse { Error = "Error authenticating to external api" });
        }
    }
    

    var request = new HttpRequestMessage(HttpMethod.Post, $"{oauthBaseUrl}{oauthProvider.GetAccessUrlPathFromProvider()}")
    {
        Headers =
        {
            { "Accept", "application/json" }
        },

        Content = oauthProvider.GetAuthorizationContent(clientId, clientId, code)
    };
    var response = await client.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();

    var jsonDoc = JsonDocument.Parse(content);
    if (!response.IsSuccessStatusCode)
    {
        return TypedResults.BadRequest(new ErrorResponse { Error = content });
    }

    if (jsonDoc.RootElement.TryGetProperty("error", out JsonElement error))
    {
        return TypedResults.BadRequest(new ErrorResponse { Error = error.GetString() ?? "" });
    }
    var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
    var tokenType = jsonDoc.RootElement.GetProperty("token_type").GetString();
    var scope = jsonDoc.RootElement.GetProperty("scope").GetString();

    return TypedResults.Ok(new TokenResponse { AccessToken = accessToken, TokenType = tokenType, Scope = scope });
});

app.MapGet(getAuthLinkEndpoint, () => new UrlResponse { Url = $"{oauthBaseUrl}{oauthProvider.GetAuthorizeUrlPathFromProvider(clientId)}"});

app.Run();