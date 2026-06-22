using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Tasks.TaskContinuationOptions;

namespace Flarial.Runtime.Services;

static partial class HttpService
{

    internal static async Task<HttpResponseMessage?> ProbeAsync(IReadOnlyList<string> uris)
    {
        using CancellationTokenSource cts = new();
        var tasks = Probe(uris, cts.Token);

        try
        {
            await foreach (var task in Task.WhenEach(tasks))
            {
                var response = await task;
                if (response is null) continue;

                cts.Cancel();
                tasks.Remove(task);

                return response;
            }
            return null;
        }
        finally
        {
            foreach (var task in tasks)
                _ = task.ContinueWith(OnContinuation, RunContinuationsAsynchronously);

            static async Task OnContinuation(Task<HttpResponseMessage?> task)
            {
                (await task)?.Dispose();
            }
        }
    }

    static List<Task<HttpResponseMessage?>> Probe(IReadOnlyList<string> uris, CancellationToken token)
    {
        List<Task<HttpResponseMessage?>> tasks = new(uris.Count);

        foreach (var uri in uris)
            tasks.Add(ProbeAsync(uri, token));

        return tasks;
    }

    static async Task<HttpResponseMessage?> ProbeAsync(string uri, CancellationToken token)
    {
        try { return await GetAsync(uri, token); }
        catch { return null; }
    }
}

static partial class HttpService
{
    [Obsolete("Don't use `HttpService.Probing`.", true)]
    static class Probing
    {
        internal static async Task<string?> ProbeAsync(IEnumerable<string> uris)
        {
            using CancellationTokenSource cts = new();
            var tasks = uris.Select(uri => ProbeAsync(uri, cts.Token));

            await foreach (var task in Task.WhenEach(tasks))
            {
                var uri = await task;
                if (uri is null) continue;

                cts.Cancel();
                return uri;
            }

            return null;
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
}
