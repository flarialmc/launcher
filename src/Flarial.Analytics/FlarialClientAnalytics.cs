using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flarial.Runtime.Core;
using Flarial.Runtime.Services;
using Windows.Security.Cryptography;
using Windows.System.Profile;

namespace Flarial.Analytics;

public static class FlarialClientAnalytics
{
    const string UserAgent = "Samsung Smart Fridge";
    const string AnalyticsUri = "https://api.flarial.xyz/launcher/events/launch";

    static readonly Uri s_uri = new(AnalyticsUri);
    static readonly string s_identifier = CryptographicBuffer.EncodeToHexString(SystemIdentification.GetSystemIdForPublisher().Id);

    extension(FlarialClient client)
    {
        public async Task<bool?> LaunchAsync()
        {
            var launched = await Task.Run(client.Launch);
            if (launched ?? false) _ = PostAsync();
            return launched;
        }
    }

    static async Task PostAsync()
    {
        var timestamp = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        var content = await JsonSerializer.SerializeAsync(new Dictionary<string, string>
        {
            ["timestamp"] = timestamp,
            ["installId"] = s_identifier
        });

        using HttpRequestMessage request = new(HttpMethod.Post, s_uri)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("User-Agent", UserAgent);
        request.Headers.Add("X-Flarial.Timestamp", timestamp);
        request.Headers.Add("X-Flarial-InstallId", s_identifier);
        request.Headers.Add("X-Flarial-Nonce", $"{Guid.NewGuid():N}");

        using (await HttpStack.SendAsync(request)) { }
    }
}
