using Flarial.Runtime.Services;

namespace Flarial.Runtime.Identity;

sealed class RefreshTokenService : CredentialService<RefreshTokenService>
{
    private protected override string Username => "Flarial";
    private protected override string Resource => "Flarial Launcher";
}