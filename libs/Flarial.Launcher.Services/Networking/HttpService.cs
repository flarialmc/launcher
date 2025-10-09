using System;
using System.IO;
using System.Net.Http;
using static System.Math;
using System.Threading.Tasks;
using static System.Net.DecompressionMethods;
using static System.Net.Http.HttpCompletionOption;
using static System.Net.Http.HttpMethod;
using static System.Environment;

namespace Flarial.Launcher.Services.Networking;

static class HttpService
{
    static readonly int _count = SystemPageSize;

    static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        AutomaticDecompression = GZip | Deflate
    });

    internal static async Task<Stream> StreamAsync(string uri) => await _httpClient.GetStreamAsync(uri);

    internal static async Task<bool> CheckAsync(string uri)
    {
        using HttpRequestMessage request = new(Head, uri);
        try
        {
            using var message = await _httpClient.SendAsync(request);
            return true;
        }
        catch { return false; }
    }

    internal static async Task DownloadAsync(string uri, string path, Action<int>? action)
    {
        using var message = await _httpClient.GetAsync(uri, ResponseHeadersRead);
        message.EnsureSuccessStatusCode();

        using var destination = File.Create(path);
        using var source = await message.Content.ReadAsStreamAsync();

        var count = 0; var value = 0L;
        var buffer = new byte[_count];
        var length = message.Content.Headers.ContentLength ?? 0;

        while ((count = await source.ReadAsync(buffer, 0, _count)) != 0)
        {
            await destination.WriteAsync(buffer, 0, count);
            if (action is { } && length > 0)
            {
                float percentage = value += count;

                percentage *= 100F;
                percentage /= length;

                action((int)Round(percentage));
            }
        }
    }
}