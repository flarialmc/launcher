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
    readonly Task<byte[]?> _task;

    public string Uri { get; }
    public string Image { get; }

    [JsonConstructor]
    internal Promotion(string uri, string image)
    {
        Uri = uri; 
        Image = image;
        _task = HttpService.TryGetBytesAsync(image);
    }

    public Task<byte[]?> GetImageAsync() => _task;

}