using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    internal static async Task<T> GetJsonAsync<T>(string uri)
    {
        using var stream = await s_client.GetStreamAsync(uri);
        return await JsonService.Default.ReadAsync<T>(stream);
    }

    internal static Task<byte[]> GetBytesAsync(string uri)
    {
        return s_client.GetByteArrayAsync(uri);
    }

    internal static async Task<byte[]?> TryGetBytesAsync(string uri)
    {
        try
        {
            using var response = await GetAsync(uri, default);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch { return null; }
    }
}