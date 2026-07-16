using System;
using System.Net.Http;
using System.Threading.Tasks;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Identity;

public sealed class AccountDetails
{

}

public static class AccountManager
{
    const string AccountUri = "https://flarial.xyz/api/v1/launcher/account";

    public static async Task<AccountDetails?> LoginAsync()
    {
        if (await OAuthManager.GetAccessTokenAsync() is not { } accessToken)
            return null;

        using HttpRequestMessage request = new(HttpMethod.Get, AccountUri);
        request.Headers.Authorization = new("Bearer", accessToken);

        using var response = await HttpService.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        
        return new();
    }
}