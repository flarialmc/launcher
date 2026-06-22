using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flarial.Runtime.Services;
using Windows.Security.Cryptography;
using Windows.System.Profile;

namespace Flarial.Runtime.Core;

public static class FlarialClientExtensions
{
    const string UserAgent = "Samsung Smart Fridge";
    const string AnalyticsUri = "https://api.flarial.xyz/launcher/events/launch";

    static readonly Uri s_uri;
    static readonly string s_identifier;

    static FlarialClientExtensions()
    {
        var info = SystemIdentification.GetSystemIdForPublisher();
        var identifier = CryptographicBuffer.EncodeToHexString(info.Id);

        s_uri = new(AnalyticsUri);
        s_identifier = identifier;
    }

    extension(FlarialClient)
    {
        public static bool? LaunchWithTracking()
        {
            var value = FlarialClient.Launch();
            if (value ?? false) _ = SendAsync();
            return value;
        }
    }

    static async Task SendAsync()
    {
        var timestamp = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var json = $"{{\"timestamp\":\"{timestamp}\",\"installId\":\"{s_identifier}\"}}";

        using StringContent content = new(json, Encoding.UTF8, "application/json");
        using HttpRequestMessage request = new(HttpMethod.Post, s_uri) { Content = content };

        request.Headers.Add("User-Agent", UserAgent);
        request.Headers.Add("X-Flarial.Timestamp", timestamp);
        request.Headers.Add("X-Flarial-InstallId", s_identifier);
        request.Headers.Add("X-Flarial-Nonce", $"{Guid.NewGuid():N}");

        using (await HttpService.SendAsync(request)) { }
    }
}