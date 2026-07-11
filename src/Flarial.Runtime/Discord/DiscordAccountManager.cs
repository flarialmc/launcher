using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Runtime.Discord;

public static class DiscordAccountManager
{
    static readonly byte[] s_response = Encoding.UTF8.GetBytes("You may close this window now.");

    const string RedirectUri = "http://localhost:65535/";
    const string AuthorizeUri = $"https://discord.com/oauth2/authorize?response_type=code&client_id=1058426966602174474&scope=identify&redirect_uri={RedirectUri}";

    public static async Task<string?> GetAuthorizationCodeAsync()
    {
        NativeMethods.ShellExecute(AuthorizeUri);

        using HttpListener listener = new();
        listener.Prefixes.Add(RedirectUri);

        listener.Start(); try
        {
            var context = await listener.GetContextAsync();

            using var stream = context.Response.OutputStream;
            context.Response.ContentLength64 = s_response.Length;

            await stream.WriteAsync(s_response);
            return context.Request.QueryString["code"];
        }
        finally { listener.Stop(); }
    }
}