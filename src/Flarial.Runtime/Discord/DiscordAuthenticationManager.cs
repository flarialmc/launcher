using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Flarial.Runtime.Identity;
using Flarial.Runtime.Services;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Runtime.Discord;

public static class DiscordAuthenticationManager
{
    static readonly ReadOnlyMemory<byte> s_response = "You may close this window now."u8.ToArray();

    const string AccessToken = "access_token";
    const string RefreshToken = "refresh_token";
    const string AuthorizationCode = "authorization_code";

    const string ClientId = "1058426966602174474";
    const string Scope = "identify guilds.members.read";

    const string TokenUri = "https://discord.com/api/oauth2/token";
    const string AuthorizeUri = $"https://discord.com/oauth2/authorize?response_type=code&code_challenge_method=S256&client_id={ClientId}&scope={Scope}&state={{0}}&code_challenge={{1}}&redirect_uri={{2}}";

    static async Task<(string AuthorizationCode, string CodeVerifier, string RedirectUri)?> GetAuthorizationAsync()
    {
        var state = RequestHelper.CreateApplicationState();
        var (verifier, challenge) = RequestHelper.CreateCodeExchange();

        var redirectUri = $"{RequestHelper.CreateRedirectUri()}/";
        var requestUri = string.Format(AuthorizeUri, state, challenge, redirectUri);

        using HttpListener listener = new();
        listener.Prefixes.Add(redirectUri);

        listener.Start(); try
        {
            NativeMethods.ShellExecute(requestUri);

            var context = await listener.GetContextAsync();
            using var stream = context.Response.OutputStream;

            context.Response.ContentLength64 = s_response.Length;
            await stream.WriteAsync(s_response);

            if (context.Request.QueryString["state"] != state)
                return null;

            if (context.Request.QueryString["code"] is not { } code)
                return null;

            return new()
            {
                CodeVerifier = verifier,
                AuthorizationCode = code,
                RedirectUri = redirectUri
            };
        }
        finally { listener.Stop(); }
    }

    static async Task<(string AccessToken, string RefreshToken)?> ParseTokensAsync(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var accessToken = document.RootElement.GetProperty(AccessToken);
        var refreshToken = document.RootElement.GetProperty(RefreshToken);

        return new()
        {
            AccessToken = accessToken.GetString()!,
            RefreshToken = refreshToken.GetString()!
        };
    }

    static async Task<(string AccessToken, string RefreshToken)?> GetTokensAsync()
    {
        if (await GetAuthorizationAsync() is not { } tuple)
            return null;

        using FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["grant_type"] = AuthorizationCode,
            ["code"] = tuple.AuthorizationCode,
            ["redirect_uri"] = tuple.RedirectUri,
            ["code_verifier"] = tuple.CodeVerifier
        });

        using var response = await HttpService.PostAsync(TokenUri, content);
        response.EnsureSuccessStatusCode();

        return await ParseTokensAsync(response);
    }

    public static async Task<bool> AuthenticateAsync()
    {
        if (await GetTokensAsync() is { } token)
        {
            RefreshTokenManager._.Set(token.RefreshToken);
            return true;
        }
        return false;
    }

    internal static async Task<string?> AuthenticateSilentlyAsync()
    {
        if (RefreshTokenManager._.Get() is not { } refreshToken)
            return null;

        using FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["grant_type"] = RefreshToken,
            ["refresh_token"] = refreshToken
        });

        using var response = await HttpService.PostAsync(TokenUri, content);

        if (!response.IsSuccessStatusCode)
        {
            RefreshTokenManager._.Remove();
            return null;
        }

        if (await ParseTokensAsync(response) is not { } token)
            return null;

        RefreshTokenManager._.Set(token.RefreshToken);
        return token.AccessToken;
    }
}