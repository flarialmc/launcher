using System;
using System.Runtime.Serialization;
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

[DataContract]
public sealed class Promotion
{
    Promotion() { }

    [DataMember]
    public string Uri { get; private set; } = null!;

    [DataMember]
    public string Image { get; private set; } = null!;
}