using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    internal static async Task<string?> ProbeAsync(IEnumerable<string> uris)
    {
        using CancellationTokenSource cts = new();

        await foreach (var task in Task.WhenEach(Probe(uris, cts.Token)))
        {
            var uri = await task;
            if (uri is null) continue;

            cts.Cancel();
            return uri;
        }
        return null;

        static IEnumerable<Task<string?>> Probe(IEnumerable<string> uris, CancellationToken token)
        {
            foreach (var uri in uris)
                yield return ProbeAsync(uri, token);
        }
    }

    static async Task<string?> ProbeAsync(string uri, CancellationToken token)
    {
        try
        {
            using var response = await GetAsync(uri, token);
            return response.IsSuccessStatusCode ? uri : null;
        }
        catch { return null; }
    }
}