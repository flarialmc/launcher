using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

static class HttpService
{
    static readonly HttpClient s_client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = int.MaxValue,
        AutomaticDecompression = DecompressionMethods.All
    }, true);

    static readonly int s_length = Environment.SystemPageSize;

    internal static async Task<byte[]> GetBytesAsync(string uri) => await s_client.GetByteArrayAsync(uri);

    internal static async Task<HttpResponseMessage> GetAsync(string uri, [Optional] CancellationToken token) => await s_client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);

    internal static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) => await s_client.SendAsync(request);

    internal static async Task<T> GetJsonAsync<T>(string uri)
    {
        using var stream = await s_client.GetStreamAsync(uri);
        return await JsonService.Default.ReadAsync<T>(stream);
    }

    internal static async Task DownloadAsync(string uri, string path, Action<int> callback)
    {
        using var response = await GetAsync(uri);
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