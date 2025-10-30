using System;
using System.IO;
using System.Net.Http;
using static System.Math;
using System.Threading.Tasks;
using static System.Environment;
using static System.Net.Http.HttpCompletionOption;
using static System.Net.DecompressionMethods;
using System.Net;
using MihaZupan;

namespace Flarial.Launcher.Services.Networking;

public static class HttpService
{
    static readonly HttpClient s_proxy = new(new HttpServiceHandler { Proxy = new HttpToSocks5Proxy($"{IPAddress.Loopback}", ushort.MaxValue) }, true);

    static readonly HttpClient s_client = new(new HttpServiceHandler(), true);

    static HttpClient HttpClient => UseProxy ? s_proxy : s_client;

    static readonly int s_length = SystemPageSize;

    public static bool UseProxy { get; set; }

    public static bool UseDnsOverHttps
    {
        get => HttpServiceHandler.UseDnsOverHttps;
        set => HttpServiceHandler.UseDnsOverHttps = value;
    }

    public static async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content) => await HttpClient.PostAsync(uri, content);

    public static async Task<HttpResponseMessage> GetAsync(string uri) => await GetAsync(uri);

    public static async Task<T> GetAsync<T>(string uri)
    {
        return (T)(object)(typeof(T) switch
        {
            var @_ when _ == typeof(string) => await HttpClient.GetStringAsync(uri),
            var @_ when _ == typeof(Stream) => await HttpClient.GetStreamAsync(uri),
            _ => throw new NotImplementedException()
        });
    }

    public static async Task DownloadAsync(string uri, string path, Action<int> action)
    {
        using var message = await HttpClient.GetAsync(uri, ResponseHeadersRead);
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