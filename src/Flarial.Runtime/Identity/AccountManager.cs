using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Identity;

public sealed class AccountDetails
{
    readonly Task<byte[]?> _task = null!;

    public string DisplayName { get; } = null!;

    public bool HasBetaAccess { get; }
    public bool HasFlarialPlus { get; }

    internal AccountDetails(string displayName, bool hasFlarialPlus, string? avatarUrl)
    {
        DisplayName = displayName;

        HasBetaAccess = hasFlarialPlus;
        HasFlarialPlus = hasFlarialPlus;

        _task = avatarUrl is null
        ? Task.FromResult<byte[]?>(null)
        : HttpService.TryGetBytesAsync(avatarUrl);
    }

    public Task<byte[]?> GetAvatarAsync() => _task;
}

public static class AccountManager
{
    const string AccountUri = "https://flarial.xyz/api/v1/launcher/account";

    public static async Task<AccountDetails?> LoginAsync()
    {
        if (await OAuthManager.GetAccessTokenAsync() is not { } accessToken)
            return null;

        using HttpRequestMessage request = new(HttpMethod.Get, AccountUri);
        request.Headers.Authorization = new("Bearer", accessToken);

        using var response = await HttpService.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return await GetAccountDetailsAsync(response);
    }

    public static Task LogoutAsync() => OAuthManager.RevokeRefreshTokenAsync();

    static async Task<AccountDetails> GetAccountDetailsAsync(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var user = document.RootElement.GetProperty("user");
        var entitlements = document.RootElement.GetProperty("entitlements");

        var avatarUrl = user.GetProperty("avatar_url").GetString();
        var displayName = user.GetProperty("display_name").GetString();

        var flarialPlus = entitlements.GetProperty("flarial_plus");
        var hasFlarialPlus = flarialPlus.GetProperty("active").GetBoolean();

        return new(displayName!, hasFlarialPlus, avatarUrl);
    }

}