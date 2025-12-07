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

    const string Uri = "https://cdn.flarial.xyz/202.txt";

    static readonly HttpClient s_client = new(new HttpServiceHandler(), true);

    static HttpClient HttpClient => UseProxy ? s_proxy : s_client;

    static readonly int s_length = SystemPageSize;

    public static bool UseProxy { get; set; }

    public static bool UseDnsOverHttps { get => HttpServiceHandler.UseDnsOverHttps; set => HttpServiceHandler.UseDnsOverHttps = value; }

    public static async Task<bool> AvailableAsync() { try { _ = await HttpClient.GetStringAsync(Uri); return true; } catch { return false; } }

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

    internal static async Task DownloadAsync(string uri, string path, Action<int> action)
    {
        using var message = await HttpClient.GetAsync(uri); message.EnsureSuccessStatusCode();
        using Stream source = await message.Content.ReadAsStreamAsync(), destination = File.Create(path);

        int count = 0; double value = 0;
        var buffer = new byte[s_length];

        while ((count = await source.ReadAsync(buffer, 0, s_length)) != 0)
        {
            await destination.WriteAsync(buffer, 0, count);
            if (action is { }) action((int)((value += count) / source.Length * 100));
        }
    }
}