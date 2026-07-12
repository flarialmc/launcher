
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Discord;

readonly struct DiscordSession(string token)
{
    const string GuildId = "1049946152092586054";
    const string TesterRoleId = "1469952430149210175";
    const string FlarialPlusRoleId = "1268949825865650268";

    const string ProfileUri = "https://discord.com/api/users/@me";
    const string GuildMemberUri = $"https://discord.com/api/users/@me/guilds/{GuildId}/member";

    async Task<HttpResponseMessage> GetAsync(string uri)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, uri);
        request.Headers.Authorization = new("Bearer", token);
        return await HttpService.SendAsync(request);
    }

    internal async Task<(string Id, string Avatar, string Username)> GetProfileAsync()
    {
        using var response = await GetAsync(ProfileUri);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var id = document.RootElement.GetProperty("id");
        var avatar = document.RootElement.GetProperty("avatar");
        var username = document.RootElement.GetProperty("username");

        return (id.GetString()!, avatar.GetString()!, username.GetString()!);
    }

    internal async Task<(bool IsFlarialPlus, bool IsTester)> GetRolesAsync()
    {
        var isTester = false;
        var isFlarialPlus = false;

        using var response = await GetAsync(GuildMemberUri);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var roles = document.RootElement.GetProperty("roles");

        foreach (var role in roles.EnumerateArray())
        {
            switch (role.GetString())
            {
                case TesterRoleId:
                    isTester = true;
                    break;

                case FlarialPlusRoleId:
                    isFlarialPlus = true;
                    break;
            }
        }

        return (isFlarialPlus, isTester);
    }
}
