using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class PromotionManager
{
    const string PromotionUri = "https://cdn.flarial.xyz/launcher/Promotions.json";

    static readonly JsonSerializer<Promotion[]> s_serializer = JsonSerializer<Promotion[]>.Get();

    public static async Task<Promotion[]> GetDetailsAsync()
    {
        try
        {
            using var stream = await HttpStack.GetStreamAsync(PromotionUri);
            return await s_serializer.DeserializeAsync(stream);
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