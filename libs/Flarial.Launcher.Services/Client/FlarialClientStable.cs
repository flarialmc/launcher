using Flarial.Launcher.Services.Modding;

namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientStable : FlarialClient
{
    internal FlarialClientStable() : base(Injector.UWP, "34F45015-6EB6-4213-ABEF-F2967818E628") { }
}