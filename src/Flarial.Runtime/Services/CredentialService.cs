using System;
using Windows.Security.Credentials;

namespace Flarial.Runtime.Services;

abstract class CredentialService<T> : CredentialService where T : CredentialService<T>, new()
{
    private protected CredentialService()
    {
        if (_ is null) return;
        throw new InvalidOperationException();
    }

    internal static readonly T _  = new();
}

abstract class CredentialService
{
    private protected CredentialService() { }

    private protected abstract string Resource { get; }
    private protected abstract string Username { get; }

    static readonly PasswordVault s_vault = new();

    PasswordCredential? Retrieve()
    {
        try { return s_vault.Retrieve(Resource, Username); }
        catch { return null; }
    }

    internal void Remove()
    {
        if (Retrieve() is { } credential)
            s_vault.Remove(credential);
    }

    internal string? Get()
    {
        if (Retrieve() is { } credential)
        {
            credential.RetrievePassword();
            return credential.Password;
        }
        return null;
    }

    internal void Set(string value)
    {
        s_vault.Add(new()
        {
            Password = value,
            Resource = Resource,
            UserName = Username
        });
    }
}