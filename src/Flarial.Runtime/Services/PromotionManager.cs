using System;
using System.IO;
using System.Runtime.Serialization;
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

    public static async Task<Stream?> GetImageStreamAsync(Promotion promotion)
    {
        try
        {
            if (!Uri.TryCreate(promotion.Image, UriKind.Absolute, out var imageUri))
                return null;

            using var response = await HttpStack.GetAsync(imageUri.ToString());
            if (!response.IsSuccessStatusCode ||
                response.Content.Headers.ContentType?.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) != true)
            {
                return null;
            }

            var buffer = new MemoryStream();
            using var stream = await response.Content.ReadAsStreamAsync();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;
            return buffer;
        }
        catch { return null; }
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
