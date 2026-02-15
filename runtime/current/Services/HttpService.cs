using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Environment;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Http.HttpCompletionOption;

namespace Flarial.Launcher.Runtime.Services;

public static class HttpService
{
    static readonly HttpClient s_client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = int.MaxValue,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    }, true);

    static readonly int s_length = SystemPageSize;

    public static async Task<Stream> GetStreamAsync(string uri) => await s_client.GetStreamAsync(uri);

    public static async Task<byte[]> GetBytesAsync(string uri) => await s_client.GetByteArrayAsync(uri);

    internal static async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content) => await s_client.PostAsync(uri, content);

    internal static async Task<HttpResponseMessage> GetAsync(string uri, [Optional] CancellationToken token) => await s_client.GetAsync(uri, ResponseHeadersRead, token);

    internal static async Task DownloadAsync(string uri, string path, Action<int> callback)
    {
        using var message = await GetAsync(uri);
        message.EnsureSuccessStatusCode();

        using var destination = File.Create(path);
        using var source = await message.Content.ReadAsStreamAsync();

        int count = 0; double value = 0;
        var buffer = new byte[s_length];
        var length = message.Content.Headers.ContentLength ?? 0;

        while ((count = await source.ReadAsync(buffer, 0, s_length)) != 0)
        {
            await destination.WriteAsync(buffer, 0, count);
            if (length > 0) callback((int)((value += count) / length * 100));
        }
    }
}