using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    internal static async Task<string?> PingAsync(IEnumerable<string> uris)
    {
        using CancellationTokenSource cts = new();
        var tasks = uris.Select(uri => PingAsync(uri, cts.Token));

        await foreach (var task in Task.WhenEach(tasks))
        {
            var uri = await task;
            if (uri is null) continue;

            cts.Cancel();
            return uri;
        }

        return null;
    }

    internal static async Task<string?> PingAsync(string uri, [Optional] CancellationToken token)
    {
        try
        {
            using var response = await GetAsync(uri, token);
            return response.IsSuccessStatusCode ? uri : null;
        }
        catch { return null; }
    }
}