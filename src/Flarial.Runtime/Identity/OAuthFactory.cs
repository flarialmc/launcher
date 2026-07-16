using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Runtime.Identity;

static class OAuthFactory
{
    const string ClientId = "flarial-desktop";
    const string Scope = "openid profile offline_access entitlements";

    const string ResourceUri = "https://flarial.xyz/api";
    const string AuthorizationUri = $"{ResourceUri}/auth/oauth2/authorize?response_type=code&code_challenge_method=S256&client_id={ClientId}&scope={Scope}&resource={ResourceUri}&state={{0}}&code_challenge={{1}}&redirect_uri={{2}}";

    internal static async Task<(string AuthorizationCode, string CodeVerifier, string RedirectUri)?> GetAuthorizationAsync()
    {
        var state = RequestHelper.CreateApplicationState();
        var (verifier, challenge) = RequestHelper.CreateCodeExchange();

        var redirectUri = RequestHelper.CreateRedirectUri();
        var requestUri = string.Format(AuthorizationUri, state, challenge, redirectUri);

        NativeMethods.ShellExecute(requestUri);

        return (string.Empty, verifier, redirectUri);
    }
}