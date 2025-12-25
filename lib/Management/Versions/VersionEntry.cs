using System;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Flarial.Launcher.Services.Management.Versions;

public abstract class VersionEntry
{
    internal abstract Task<string> UriAsync();

    public async Task<InstallRequest> InstallAsync(Action<int> action) => new(await UriAsync(), action);

    private protected static readonly DataContractJsonSerializerSettings s_settings = new() { UseSimpleDictionaryFormat = true };
}