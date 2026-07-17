using Flarial.Runtime.Services;

namespace Flarial.Runtime.Discord;

sealed class DiscordRefreshTokenManager : CredentialService<DiscordRefreshTokenManager>
{
    private protected override string Username => "Discord";
    private protected override string Resource => "Flarial Launcher";
}