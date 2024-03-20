using OauthRelay.Enums;

namespace OauthRelay;

public static class OauthExtensions
{
    public static string GetAuthorizeUrlPathFromProvider(this OauthProvider provider, string clientId)
    {
        return provider switch
        {
            OauthProvider.Github => $"/login/oauth/authorize?client_id={clientId}&scope=user",
            _ => throw new ArgumentException("Invalid OauthProvider")
        };
    }
    
    public static string GetAccessUrlPathFromProvider(this OauthProvider provider)
    {
        return provider switch
        {
            OauthProvider.Github => "/login/oauth/access_token",
            _ => throw new ArgumentException("Invalid OauthProvider")
        };
    }
    
    public static HttpContent GetAuthorizationContent(this OauthProvider provider, string clientId, string clientSecret, string authCode)
    {
        return provider switch
        {
            OauthProvider.Github => new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("code", authCode)
            }),
            _ => throw new ArgumentException("Invalid OauthProvider")
        };
    }
}