using Windows.Security.Credentials;

namespace Flarial.Runtime.Identity;

static class RefreshTokenManager
{
    const string UserName = "Flarial";
    const string Resource = "Flarial Launcher";

    static readonly PasswordVault s_vault = new();

    static PasswordCredential? RetrieveToken()
    {
        try { return s_vault.Retrieve(Resource, UserName); }
        catch { return null; }
    }

    internal static void RemoveToken()
    {
        if (RetrieveToken() is { } credential)
            s_vault.Remove(credential);
    }

    internal static string? GetToken()
    {
        if (RetrieveToken() is { } credential)
        {
            credential.RetrievePassword();
            return credential.Password;
        }
        return null;
    }

    internal static void SetToken(string refreshToken)
    {
        s_vault.Add(new()
        {
            Resource = Resource,
            UserName = UserName,
            Password = refreshToken
        });
    }
}