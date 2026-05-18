using System;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class PromotionManager
{
    const string PromotionsUri = "https://cdn.flarial.xyz/launcher/Promotions.json";

    public static async Task<Promotion[]> GetAsync()
    {
        try
        {
            using var stream = await HttpStack.GetStreamAsync(PromotionsUri);
            return await JsonSerializer.DeserializeAsync<Promotion[]>(stream);
        }
        catch { return []; }
    }
}

public sealed class Promotion
{
    public string Uri { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;
}
