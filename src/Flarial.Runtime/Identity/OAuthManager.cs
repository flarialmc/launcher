using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Flarial.Runtime.Services;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Runtime.Identity;

public static class OAuthManager
{
    static readonly byte[] s_response = Encoding.UTF8.GetBytes("You may close this window now.");

    const string AccessToken = "access_token";
    const string RefreshToken = "refresh_token";
    const string AuthorizationCode = "authorization_code";

    const string ClientId = "flarial-desktop";
    const string Resource = "https://flarial.xyz/api";
    const string Scope = "openid profile offline_access entitlements";

    const string TokenUri = $"{Resource}/auth/oauth2/token";
    const string RevokeUri = $"{Resource}/auth/oauth2/revoke";
    const string AuthorizeUri = $"{Resource}/auth/oauth2/authorize?response_type=code&code_challenge_method=S256&client_id={ClientId}&scope={Scope}&resource={Resource}&state={{0}}&code_challenge={{1}}&redirect_uri={{2}}";

    static async Task<(string AuthorizationCode, string CodeVerifier, string RedirectUri)?> GetAuthorizationAsync()
    {
        var state = RequestHelper.CreateApplicationState();
        var (verifier, challenge) = RequestHelper.CreateCodeExchange();

        var redirectUri = $"{RequestHelper.CreateRedirectUri()}/oauth/callback";
        var requestUri = string.Format(AuthorizeUri, state, challenge, redirectUri);

        using HttpListener listener = new();
        listener.Prefixes.Add($"{redirectUri}/");

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

            return (code, verifier, redirectUri);
        }
        finally { listener.Stop(); }
    }

    static async Task<(string AccessToken, string RefreshToken)?> ParseTokensAsync(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var accessToken = document.RootElement.GetProperty(AccessToken);
        var refreshToken = document.RootElement.GetProperty(RefreshToken);

        return (accessToken.GetString()!, refreshToken.GetString()!);
    }

    static async Task<(string AccessToken, string RefreshToken)?> GetTokensAsync()
    {
        if (await GetAuthorizationAsync() is not { } tuple)
            return null;

        using FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            ["resource"] = Resource,
            ["client_id"] = ClientId,
            ["grant_type"] = AuthorizationCode,
            ["code"] = tuple.AuthorizationCode,
            ["redirect_uri"] = tuple.RedirectUri,
            ["code_verifier"] = tuple.CodeVerifier,
        });

        using var response = await HttpService.PostAsync(TokenUri, content);
        if (!response.IsSuccessStatusCode) return null;

        return await ParseTokensAsync(response);
    }

    public static async Task<bool> AuthenticateAsync()
    {
        if (await GetTokensAsync() is { } tuple)
        {
            RefreshTokenService._.Set(tuple.RefreshToken);
            return true;
        }
        return false;
    }

    internal static async Task<string?> GetAccessTokenAsync()
    {
        if (RefreshTokenService._.Get() is not { } refreshToken)
            return null;

        using FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            ["resource"] = Resource,
            ["client_id"] = ClientId,
            ["grant_type"] = RefreshToken,
            ["refresh_token"] = refreshToken
        });

        using var response = await HttpService.PostAsync(TokenUri, content);

        if (!response.IsSuccessStatusCode)
        {
            RefreshTokenService._.Remove();
            return null;
        }

        if (await ParseTokensAsync(response) is not { } tuple)
            return null;

        RefreshTokenService._.Set(tuple.RefreshToken);
        return tuple.AccessToken;
    }

    internal static async Task RevokeRefreshTokenAsync()
    {
        if (RefreshTokenService._.Get() is { } refreshToken)
        {
            RefreshTokenService._.Remove();

            using FormUrlEncodedContent content = new(new Dictionary<string, string>
            {
                ["token"] = refreshToken,
                ["resource"] = Resource,
                ["client_id"] = ClientId,
                ["token_type_hint"] = RefreshToken
            });

            using var response = await HttpService.PostAsync(RevokeUri, content);
            response.EnsureSuccessStatusCode();
        }
    }
}