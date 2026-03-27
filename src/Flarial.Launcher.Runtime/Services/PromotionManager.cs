using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Flarial.Launcher.Runtime.Services;

public static class PromotionManager
{
    const string PromotionUri = "https://cdn.flarial.xyz/launcher/Promotions.json";

    static readonly JsonSerializer<Promotion[]> s_serializer = JsonSerializer<Promotion[]>.Get();

    public static async Task<Promotion[]> GetDetailsAsync()
    {
        try
        {
            using var stream = await HttpService.GetStreamAsync(PromotionUri);
            return await Task.Run(() => s_serializer.Deserialize(stream));
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