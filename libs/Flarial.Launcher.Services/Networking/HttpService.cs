using System;
using System.IO;
using System.Net.Http;
using static System.Math;
using System.Threading.Tasks;
using static System.Environment;
using static System.Net.Http.HttpCompletionOption;
using static System.Net.DecompressionMethods;
using System.Net;

namespace Flarial.Launcher.Services.Networking;

static class HttpService
{
    static readonly int s_length = SystemPageSize;

    static readonly HttpClient s_client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        AutomaticDecompression = GZip | Deflate
    }, true);

    internal static async Task<Stream> StreamAsync(string uri) => await s_client.GetStreamAsync(uri);

    internal static async Task<string> StringAsync(string uri) => await s_client.GetStringAsync(uri);

    internal static async Task DownloadAsync(string uri, string path, Action<int> action)
    {
        using var message = await s_client.GetAsync(uri, ResponseHeadersRead);
        message.EnsureSuccessStatusCode();

        var buffer = new byte[s_length];
        int count = new(), value = new();
        double length = message.Content.Headers.ContentLength ?? new();

        using var destination = File.Create(path);
        using var source = await message.Content.ReadAsStreamAsync();

        while ((count = await source.ReadAsync(buffer, 0, s_length)) != 0)
        {
            await destination.WriteAsync(buffer, 0, count);
            if (action is { } && length != 0)
            {
                double parameter = value += count;
                parameter /= length; parameter *= 100D;
                action((int)Round(parameter));
            }
        }
    }
}