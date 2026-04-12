using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class PromotionRegistry
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

[DataContract]
public sealed class Promotion
{
    Promotion() { }

    [DataMember]
    public readonly string Uri = null!;

    [DataMember]
    public readonly string Image = null!;
}