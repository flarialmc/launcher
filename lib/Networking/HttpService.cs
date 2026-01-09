using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Environment;
using System.Net;
using MihaZupan;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Http.HttpCompletionOption;

namespace Flarial.Launcher.Services.Networking;

public static class HttpService
{
    static readonly HttpClient s_proxy = new(new HttpServiceHandler
    {
        Proxy = new HttpToSocks5Proxy($"{IPAddress.Loopback}", ushort.MaxValue)
    }, true);

    static readonly HttpClient s_client = new(new HttpServiceHandler(), true);

    static HttpClient HttpClient => UseProxy ? s_proxy : s_client;

    static readonly int s_length = SystemPageSize;

    public static bool UseProxy { internal get; set { if (!field) field = value; } }

    public static bool UseDnsOverHttps { set => HttpServiceHandler.UseDnsOverHttps = value; }

    public static async Task<Stream> StreamAsync(string uri) => await HttpClient.GetStreamAsync(uri);

    internal static async Task<string> StringAsync(string uri) => await HttpClient.GetStringAsync(uri);

    public static async Task<byte[]> BytesAsync(string uri) => await HttpClient.GetByteArrayAsync(uri);

    internal static async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content) => await HttpClient.PostAsync(uri, content);

    internal static async Task<HttpResponseMessage> GetAsync(string uri, [Optional] CancellationToken token) => await HttpClient.GetAsync(uri, ResponseHeadersRead, token);

    [Obsolete("", true)]
    public static async Task<T> GetAsync<T>(string uri)
    {
        return (T)(object)(typeof(T) switch
        {
            var @_ when _ == typeof(string) => await HttpClient.GetStringAsync(uri),
            var @_ when _ == typeof(Stream) => await HttpClient.GetStreamAsync(uri),
            var @_ when _ == typeof(byte[]) => await HttpClient.GetByteArrayAsync(uri),
            _ => throw new NotImplementedException()
        });
    }

    internal static async Task DownloadAsync(string uri, string path, Action<int> action)
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
            if (length > 0) action((int)((value += count) / length * 100));
        }
    }

    const string Uri = "https://cdn.flarial.xyz/202.txt";

    public static async Task<bool> IsAvailableAsync()
    {
        try
        {
            using var message = await GetAsync(Uri);
            return message.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}