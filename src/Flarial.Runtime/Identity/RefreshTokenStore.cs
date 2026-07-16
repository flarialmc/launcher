using Windows.Security.Credentials;

namespace Flarial.Runtime.Identity;

static class RefreshTokenStore
{
    const string UserName = "Flarial";
    const string Resource = "Flarial Launcher";

    static readonly PasswordVault s_vault = new();

    internal static void Delete()
    {
        if (Find() is { } credential)
            s_vault.Remove(credential);
    }

    internal static PasswordCredential? Find()
    {
        try { return s_vault.Retrieve(Resource, UserName); }
        catch { return null; }
    }

    internal static string? Load()
    {
        if (Find() is { } credential)
        {
            credential.RetrievePassword();
            return credential.Password;
        }
        return null;
    }

    internal static void Save(string refreshToken)
    {
        s_vault.Add(new()
        {
            Resource = Resource,
            UserName = UserName,
            Password = refreshToken,
        });
    }
}