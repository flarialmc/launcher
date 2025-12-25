using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Environment;
using System.Net;
using MihaZupan;

namespace Flarial.Launcher.Services.Networking;

public class HttpService
{
    static readonly HttpClient s_proxy = new(new HttpServiceHandler
    {
        Proxy = new HttpToSocks5Proxy($"{IPAddress.Loopback}", ushort.MaxValue)
    }, true);

    static readonly HttpClient s_client = new(new HttpServiceHandler(), true);

    static HttpClient HttpClient => UseProxy ? s_proxy : s_client;

    static readonly int s_length = SystemPageSize;

    public static bool UseProxy { get; set; }

    public static bool UseDnsOverHttps { get => HttpServiceHandler.UseDnsOverHttps; set => HttpServiceHandler.UseDnsOverHttps = value; }

    internal static async Task<HttpResponseMessage> GetAsync(string uri) => await HttpClient.GetAsync(uri);

    internal static async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content) => await HttpClient.PostAsync(uri, content);

    internal static async Task<T> GetAsync<T>(string uri)
    {
        return (T)(object)(typeof(T) switch
        {
            var @_ when _ == typeof(string) => await HttpClient.GetStringAsync(uri),
            var @_ when _ == typeof(Stream) => await HttpClient.GetStreamAsync(uri),
            _ => throw new NotImplementedException()
        });
    }

    internal static async Task DownloadAsync(string uri, string path, Action<int>? action)
    {
        using var message = await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        message.EnsureSuccessStatusCode();

        using var destination = File.Create(path);
        using var source = await message.Content.ReadAsStreamAsync();

        int count = 0; double value = 0;
        var buffer = new byte[s_length];
        var length = message.Content.Headers.ContentLength ?? 0;

        while ((count = await source.ReadAsync(buffer, 0, s_length)) != 0)
        {
            await destination.WriteAsync(buffer, 0, count);
            if (action is { } && length > 0) action((int)((value += count) / length * 100));
        }
    }

    const string Uri = "https://cdn.flarial.xyz/202.txt";

    public static async Task<bool> IsAvailableAsync()
    {
        try { _ = await HttpClient.GetStringAsync(Uri); return true; }
        catch { return false; }
    }
}