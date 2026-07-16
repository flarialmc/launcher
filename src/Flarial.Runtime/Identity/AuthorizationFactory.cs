namespace Flarial.Runtime.Identity;

static class AuthorizationFactory
{
    const string Scope = "openid profile offline_access entitlements";
    const string AuthorizationUri = "https://flarial.xyz/api/auth/oauth2/authorize";
}