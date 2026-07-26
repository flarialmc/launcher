using System;
using System.Buffers.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Flarial.Runtime.Services;
using NSec.Cryptography;
using Windows.Security.Cryptography;
using Windows.System.Profile;

namespace Flarial.Runtime.Core;

partial class FlarialClient
{
    const string UserAgent = "Flarial-Launcher/2.0";
    const string InstallationsUri = "https://api.flarial.xyz/api/v2/installations";
    const string EventsUri = "https://api.flarial.xyz/api/v2/events";
    const string EventsPath = "/api/v2/events";

    static readonly string s_identifier;
    static readonly Lazy<Task<InstallationIdentity>> s_identity = new(LoadOrRegisterAsync);

    static FlarialClient()
    {
        var info = SystemIdentification.GetSystemIdForPublisher();
        s_identifier = CryptographicBuffer.EncodeToHexString(info.Id);
    }

    static async Task PostAnalyticsAsync()
    {
        try
        {
            var identity = await s_identity.Value;
            var occurredAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
            var batch = new EventBatch
            {
                BatchId = $"{Guid.NewGuid():N}",
                Events =
                [
                    new LaunchEvent
                    {
                        Type = "launcher_launch",
                        OccurredAtMs = occurredAt,
                        Properties = new() { LegacyInstallId = s_identifier, Version = version }
                    }
                ]
            };
            var body = JsonSerializer.SerializeToUtf8Bytes(batch, AnalyticsJsonContext.Default.EventBatch);
            var timestamp = $"{occurredAt / 1000}";
            var nonce = $"{Guid.NewGuid():N}";
            var digest = Base64Url.EncodeToString(SHA256.HashData(body));
            var canonical = Encoding.UTF8.GetBytes($"POST\n{EventsPath}\n{timestamp}\n{nonce}\n{digest}");
            var signature = SignatureAlgorithm.Ed25519.Sign(identity.Key, canonical);

            using ByteArrayContent content = new(body);
            content.Headers.ContentType = new("application/json");
            using HttpRequestMessage request = new(HttpMethod.Post, EventsUri) { Content = content };
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("X-Flarial-Installation", identity.InstallationId);
            request.Headers.Add("X-Flarial-Timestamp", timestamp);
            request.Headers.Add("X-Flarial-Nonce", nonce);
            request.Headers.Add("X-Flarial-Signature", Base64Url.EncodeToString(signature));

            using var response = await HttpService.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.Accepted)
                throw new HttpRequestException($"Event endpoint returned {response.StatusCode}");
        }
        catch
        {
            // Telemetry is best-effort and must never affect launching the client.
        }
    }

    static async Task<InstallationIdentity> LoadOrRegisterAsync()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Flarial",
            "Launcher");
        var path = Path.Combine(directory, "api-v2.json");

        try
        {
            await using var stream = File.OpenRead(path);
            var stored = await JsonSerializer.DeserializeAsync(
                stream,
                AnalyticsJsonContext.Default.StoredIdentity);
            if (stored is not null)
            {
                var privateKey = Base64Url.DecodeFromChars(stored.PrivateKey);
                var key = Key.Import(
                    SignatureAlgorithm.Ed25519,
                    privateKey,
                    KeyBlobFormat.RawPrivateKey,
                    new() { ExportPolicy = KeyExportPolicies.AllowPlaintextArchiving });
                return new(stored.InstallationId, key);
            }
        }
        catch
        {
            // Missing or invalid identity files are replaced by a fresh registration.
        }

        var creation = new KeyCreationParameters
        {
            ExportPolicy = KeyExportPolicies.AllowPlaintextArchiving
        };
        var signingKey = Key.Create(SignatureAlgorithm.Ed25519, creation);
        var publicKey = signingKey.PublicKey.Export(KeyBlobFormat.RawPublicKey);
        var registration = new InstallationRequest
        {
            PublicKey = Base64Url.EncodeToString(publicKey),
            Platform = "launcher",
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown"
        };
        var body = JsonSerializer.SerializeToUtf8Bytes(
            registration,
            AnalyticsJsonContext.Default.InstallationRequest);

        using ByteArrayContent content = new(body);
        content.Headers.ContentType = new("application/json");
        using HttpRequestMessage request = new(HttpMethod.Post, InstallationsUri) { Content = content };
        request.Headers.Add("User-Agent", UserAgent);
        using var response = await HttpService.SendAsync(request);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            signingKey.Dispose();
            throw new HttpRequestException($"Installation endpoint returned {response.StatusCode}");
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var registered = await JsonSerializer.DeserializeAsync(
            responseStream,
            AnalyticsJsonContext.Default.InstallationResponse)
            ?? throw new InvalidDataException("Installation response was empty");
        if (string.IsNullOrWhiteSpace(registered.InstallationId))
            throw new InvalidDataException("Installation response omitted installationId");

        Directory.CreateDirectory(directory);
        var storedIdentity = new StoredIdentity
        {
            InstallationId = registered.InstallationId,
            PrivateKey = Base64Url.EncodeToString(signingKey.Export(KeyBlobFormat.RawPrivateKey))
        };
        var temporary = $"{path}.tmp";
        await using (var stream = File.Create(temporary))
        {
            await JsonSerializer.SerializeAsync(
                stream,
                storedIdentity,
                AnalyticsJsonContext.Default.StoredIdentity);
        }
        File.Move(temporary, path, true);

        return new(registered.InstallationId, signingKey);
    }
}

sealed record InstallationIdentity(string InstallationId, Key Key);

sealed class StoredIdentity
{
    [JsonPropertyName("installationId")]
    public required string InstallationId { get; init; }

    [JsonPropertyName("privateKey")]
    public required string PrivateKey { get; init; }
}

sealed class InstallationRequest
{
    [JsonPropertyName("publicKey")]
    public required string PublicKey { get; init; }

    [JsonPropertyName("platform")]
    public required string Platform { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

sealed class InstallationResponse
{
    [JsonPropertyName("installationId")]
    public string InstallationId { get; init; } = string.Empty;
}

sealed class EventBatch
{
    [JsonPropertyName("batchId")]
    public required string BatchId { get; init; }

    [JsonPropertyName("events")]
    public required LaunchEvent[] Events { get; init; }
}

sealed class LaunchEvent
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("occurredAtMs")]
    public required long OccurredAtMs { get; init; }

    [JsonPropertyName("properties")]
    public required LaunchProperties Properties { get; init; }
}

sealed class LaunchProperties
{
    [JsonPropertyName("legacyInstallId")]
    public required string LegacyInstallId { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

[JsonSerializable(typeof(StoredIdentity))]
[JsonSerializable(typeof(InstallationRequest))]
[JsonSerializable(typeof(InstallationResponse))]
[JsonSerializable(typeof(EventBatch))]
sealed partial class AnalyticsJsonContext : JsonSerializerContext;
