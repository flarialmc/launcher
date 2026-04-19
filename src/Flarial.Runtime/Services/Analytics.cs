using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class Analytics
{
    const string UserAgent = "Samsung Smart Fridge";

    static readonly bool s_enabled =
        !string.IsNullOrEmpty(AnalyticsBuildConfig.Secret) &&
        !string.IsNullOrEmpty(AnalyticsBuildConfig.Endpoint);

    static readonly byte[] s_key = s_enabled
        ? Encoding.UTF8.GetBytes(AnalyticsBuildConfig.Secret)
        : Array.Empty<byte>();

    public static void TrackLaunch(string installId)
    {
        if (!s_enabled || string.IsNullOrEmpty(installId)) return;
        _ = Task.Run(() => PostLaunchAsync(installId));
    }

    static async Task PostLaunchAsync(string installId)
    {
        try
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString("N");
            var canonical = ts + "." + nonce + "." + installId;

            string signature;
            using (var hmac = new HMACSHA256(s_key))
            {
                var sig = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                signature = BitConverter.ToString(sig).Replace("-", "").ToLowerInvariant();
            }

            var body = "{\"installId\":\"" + installId + "\",\"timestamp\":" + ts + "}";

            using var request = new HttpRequestMessage(HttpMethod.Post, AnalyticsBuildConfig.Endpoint)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            request.Headers.TryAddWithoutValidation("X-Flarial-Timestamp", ts);
            request.Headers.TryAddWithoutValidation("X-Flarial-Nonce", nonce);
            request.Headers.TryAddWithoutValidation("X-Flarial-InstallId", installId);
            request.Headers.TryAddWithoutValidation("X-Flarial-Signature", signature);

            using var response = await HttpStack.SendAsync(request);
            _ = response;
        }
        catch
        {
            // Fire-and-forget; never block or surface errors to the launch path.
        }
    }
}
