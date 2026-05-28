using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class PromotionService
{
    const string PromotionsUri = "https://cdn.flarial.xyz/launcher/Promotions.json";

    public static async Task<Promotion[]> GetAsync()
    {
        try
        {
            using var stream = await HttpService.GetStreamAsync(PromotionsUri);
            return await JsonService.ReadAsync<Promotion[]>(stream);
        }
        catch { return []; }
    }
}

public sealed class Promotion
{
    public string Uri { get; }
    public string Image { get; }

    [JsonConstructor]
    internal Promotion(string uri, string image) => (Uri, Image) = (uri, image);

    public async Task<Stream> GetAsync() => new MemoryStream(await HttpService.GetBytesAsync(Image));
}