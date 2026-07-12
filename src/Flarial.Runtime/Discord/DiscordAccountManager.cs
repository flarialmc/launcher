using System;
using System.Threading.Tasks;

namespace Flarial.Runtime.Discord;

public sealed class DiscordAccount
{
    public string Username { get; }
    public bool HasBetaAccess { get; }

    internal DiscordAccount(string username, bool hasBetaAccess)
    {
        Username = username;
        HasBetaAccess = hasBetaAccess;
    }
}

public static class DiscordAccountManager
{
    public static async Task<DiscordAccount?> LoginAsync()
    {
        if (await OAuthManager.AuthenticateSilentlyAsync() is not { } token)
            return null;

        DiscordSession session = new(token);
        var hasBetaAccessTask = session.HasBetaAccessAsync();
        var accountProfileTask = session.GetAccountProfileAsync();

        await Task.WhenAll(accountProfileTask, hasBetaAccessTask);

        if (await accountProfileTask is not { } accountProfile)
            return null;

        if (await hasBetaAccessTask is not { } hasBetaAccess)
            return null;

        return new(accountProfile.Username, hasBetaAccess);
    }

    public static void Logout() => CredentialManager.Remove();
}