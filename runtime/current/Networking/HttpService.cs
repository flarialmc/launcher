using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Environment;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Http.HttpCompletionOption;

namespace Flarial.Launcher.Runtime.Networking;

public static class HttpService
{
    static readonly HttpClient s_proxy = new(new DnsOverHttpsHandler
    {
        Proxy = new WebProxy($"{IPAddress.Loopback}", ushort.MaxValue)
    }, true);

    static readonly HttpClient s_client = new(new DnsOverHttpsHandler(), true);

    static HttpClient HttpClient => (bool)UseProxy! ? s_proxy : s_client;

    static readonly int s_length = SystemPageSize;

    public static bool? UseProxy
    {
        set => field ??= value;
        internal get => field ??= false;
    }

    public static async Task<Stream> GetStreamAsync(string url) => await HttpClient.GetStreamAsync(url);

    public static async Task<byte[]> GetBytesAsync(string url) => await HttpClient.GetByteArrayAsync(url);

    internal static async Task<HttpResponseMessage> PostAsync(string url, HttpContent content) => await HttpClient.PostAsync(url, content);

    internal static async Task<HttpResponseMessage> GetAsync(string url, [Optional] CancellationToken token) => await HttpClient.GetAsync(url, ResponseHeadersRead, token);

    internal static async Task DownloadAsync(string url, string path, Action<int> action)
    {
        using var message = await GetAsync(url);
        message.EnsureSuccessStatusCode();

        using var destination = File.Create(path);
        using var source = await message.Content.ReadAsStreamAsync();

        int count = 0; double value = 0;
        var buffer = new byte[s_length];
        var length = message.Content.Headers.ContentLength ?? 0;

        while ((count = await source.ReadAsync(buffer, 0, s_length)) != 0)
        {
            await destination.WriteAsync(buffer, 0, count);
            if (length > 0) action((int)((value += count) / length * 100));
        }
    }
}