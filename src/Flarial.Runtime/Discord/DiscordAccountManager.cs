using System.Threading.Tasks;
using Flarial.Runtime.Identity;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Discord;

public sealed class DiscordAccount
{
    readonly Task<byte[]?> _task;

    public string Username { get; }
    public bool HasBetaAccess { get; }
    public bool HasFlarialPlus { get; }

    internal DiscordAccount(string username, (bool IsFlarialPlus, bool IsTester) roles, Task<byte[]?> task)
    {
        _task = task;
        Username = username;
        HasFlarialPlus = roles.IsFlarialPlus;
        HasBetaAccess = roles.IsFlarialPlus || roles.IsTester;
    }

    public Task<byte[]?> GetAvatarAsync() => _task;
}

public static class DiscordAccountManager
{
    const string AvatarUri = "https://cdn.discordapp.com/avatars/{0}/{1}";

    public static async Task<DiscordAccount?> LoginAsync()
    {
        if (await DiscordAuthenticationManager.AuthenticateSilentlyAsync() is not { } token)
            return null;

        DiscordSession session = new(token);

        var rolesTask = session.GetRolesAsync();
        var profileTask = session.GetProfileAsync();

        await Task.WhenAll(rolesTask, profileTask);
        if (await profileTask is not { } profile) return null;

        var uri = string.Format(AvatarUri, profile.Id, profile.Avatar);
        return new(profile.Username, await rolesTask, GetBytesAsync(uri));
    }

    static async Task<byte[]?> GetBytesAsync(string uri)
    {
        using var response = await HttpService.GetAsync(uri, default);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync();
    }

    public static void Logout() => RefreshTokenManager._.Remove();
}