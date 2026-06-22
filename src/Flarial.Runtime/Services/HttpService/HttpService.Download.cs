using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    internal static async Task DownloadAsync(string uri, string path, Action<int> callback)
    {
        using var response = await GetAsync(uri);
        await DownloadAsync(response, path, callback);
    }

    internal static async Task DownloadAsync(HttpResponseMessage response, string path, Action<int> callback)
    {
        response.EnsureSuccessStatusCode();

        using var destination = File.Create(path);
        using var source = await response.Content.ReadAsStreamAsync();

        int count = 0;
        double value = 0;

        var buffer = new byte[s_length];
        var length = response.Content.Headers.ContentLength ?? 0;

        while ((count = await source.ReadAsync(buffer)) != 0)
        {
            var memory = buffer.AsMemory(0, count);
            await destination.WriteAsync(memory);

            if (length <= 0)
                continue;

            var arg = value += count;
            arg = arg / length * 100;

            callback((int)arg);
        }
    }
}