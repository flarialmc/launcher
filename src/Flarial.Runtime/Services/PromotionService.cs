using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class PromotionService
{
    const string PromotionsUri = "https://cdn.flarial.xyz/launcher/Promotions.json";

    public static async Task<Promotion[]> GetAsync()
    {
        try { return await HttpService.GetJsonAsync<Promotion[]>(PromotionsUri); }
        catch { return []; }
    }
}

public sealed class Promotion
{
    public string Uri { get; }

    [JsonInclude]
    internal string Image { get; }

    [JsonConstructor]
    internal Promotion(string uri, string image)
    {
        Uri = uri;
        Image = image;
    }

    public async Task<byte[]?> GetImageAsync()
    {
        try { return await HttpService.GetBytesAsync(Image); }
        catch { return null; }
    }
}