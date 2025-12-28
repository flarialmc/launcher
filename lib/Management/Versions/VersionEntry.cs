using System;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Flarial.Launcher.Services.Management.Versions;

public abstract class VersionEntry
{
    internal abstract Task<string> GetAsync();

    public async Task<InstallRequest> InstallAsync(Action<AppInstallState, int> action) => new(await GetAsync(), action);

    private protected static readonly DataContractJsonSerializerSettings s_settings = new() { UseSimpleDictionaryFormat = true };
}