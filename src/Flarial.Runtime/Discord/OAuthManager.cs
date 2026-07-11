using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Flarial.Runtime.Services;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Runtime.Discord;

public static class OAuthManager
{
    static readonly byte[] s_response = Encoding.UTF8.GetBytes("You may close this window now.");

    const string ClientId = "1058426966602174474";
    const string RedirectUri = "http://localhost:65535/";

    const string TokenUri = "https://discord.com/api/oauth2/token";
    const string AuthorizeUri = $"https://discord.com/oauth2/authorize?response_type=code&scope=identify&client_id={ClientId}&redirect_uri={RedirectUri}";

    static string CreateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        var sha256 = SHA256.HashData(bytes);
        var base64 = Convert.ToBase64String(sha256);

        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    static async Task<string?> GetAuthorizationCodeAsync()
    {
        string state = CreateCodeVerifier(), challenge = CreateCodeVerifier();
        var uri = $"{AuthorizeUri}&state={state}&code_challenge={challenge}&code_challenge_method=S256";

        NativeMethods.ShellExecute(uri);

        using HttpListener listener = new();
        listener.Prefixes.Add(RedirectUri);

        listener.Start(); try
        {
            var context = await listener.GetContextAsync();
            using var stream = context.Response.OutputStream;

            context.Response.ContentLength64 = s_response.Length;
            await stream.WriteAsync(s_response);

            if (context.Request.QueryString["state"] != state)
                return null;

            return context.Request.QueryString["code"];
        }
        finally { listener.Stop(); }
    }

    public static async Task GetTokenAsync()
    {
        if (await GetAuthorizationCodeAsync() is not { } authorizationCode)
            return;
    }
}