using Flarial.Runtime.Services;

namespace Flarial.Runtime.Identity;

sealed class RefreshTokenManager : CredentialService<RefreshTokenManager>
{
    private protected override string Username => "Discord";
    private protected override string Resource => "Flarial Launcher";
}