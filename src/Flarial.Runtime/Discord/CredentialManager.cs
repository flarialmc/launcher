using Windows.Security.Credentials;

namespace Flarial.Runtime.Discord;

static class CredentialManager
{
    const string UserName = "Discord";
    const string Resource = "Flarial Launcher";

    static readonly PasswordVault s_vault = new();

    internal static void Remove()
    {
        if (Get() is { } credential)
            s_vault.Remove(credential);
    }

    internal static PasswordCredential? Get()
    {
        try
        {
            var credential = s_vault.Retrieve(Resource, UserName);
            credential.RetrievePassword(); return credential;
        }
        catch { return null; }
    }

    internal static void Set(string password)
    {
        s_vault.Add(new()
        {
            Resource = Resource,
            UserName = UserName,
            Password = password,
        });
    }
}