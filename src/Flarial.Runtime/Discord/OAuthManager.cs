using System.Buffers.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Flarial.Runtime.Services;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Runtime.Discord;

public static class OAuthManager
{
    static readonly byte[] s_response = Encoding.UTF8.GetBytes("You may close this window now.");

    const string ClientId = "1058426966602174474";
    const string Scope = "identify guilds.members.read";
    const string RedirectUri = "http://localhost:65535/";

    const string TokenUri = "https://discord.com/api/oauth2/token";
    const string AuthorizeUri = $"https://discord.com/oauth2/authorize?response_type=code&scope={Scope}&client_id={ClientId}&redirect_uri={RedirectUri}";

    static async Task<(string Verifier, string Code)?> GetAuthorizationCodeAsync()
    {
        var verifier = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var challenge = Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

        var state = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var uri = $"{AuthorizeUri}&code_challenge={challenge}&code_challenge_method=S256&state={state}";

        using HttpListener listener = new();
        listener.Prefixes.Add(RedirectUri);

        listener.Start(); try
        {
            NativeMethods.ShellExecute(uri);

            var context = await listener.GetContextAsync();
            using var stream = context.Response.OutputStream;

            context.Response.ContentLength64 = s_response.Length;
            await stream.WriteAsync(s_response);

            if (context.Request.QueryString["state"] != state)
                return null;

            if (context.Request.QueryString["code"] is not { } code)
                return null;

            return (verifier, code);
        }
        finally
        {
            listener.Stop();
        }
    }

    static async Task<(string AccessToken, string RefreshToken)?> ParseTokenAsync(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var access = document.RootElement.GetProperty("access_token");
        var refresh = document.RootElement.GetProperty("refresh_token");

        return (access.GetString()!, refresh.GetString()!);
    }

    static async Task<(string AccessToken, string RefreshToken)?> GetTokenAsync()
    {
        if (await GetAuthorizationCodeAsync() is not { } tuple)
            return null;

        using FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            ["code"] = tuple.Code,
            ["client_id"] = ClientId,
            ["redirect_uri"] = RedirectUri,
            ["code_verifier"] = tuple.Verifier,
            ["grant_type"] = "authorization_code"
        });

        using var response = await HttpService.PostAsync(TokenUri, content);
        response.EnsureSuccessStatusCode();

        return await ParseTokenAsync(response);
    }

    public static async Task<bool> AuthenticateAsync()
    {
        if (await GetTokenAsync() is { } token)
        {
            CredentialManager.Set(token.RefreshToken);
            return true;
        }
        return false;
    }

    internal static async Task<string?> AuthenticateSilentlyAsync()
    {
        if (CredentialManager.Get() is not { } credential)
            return null;

        using FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = credential.Password
        });

        using var response = await HttpService.PostAsync(TokenUri, content);

        if (!response.IsSuccessStatusCode)
        {
            CredentialManager.Remove();
            return null;
        }

        if (await ParseTokenAsync(response) is not { } token)
            return null;

        CredentialManager.Set(token.RefreshToken);
        return token.AccessToken;
    }
}