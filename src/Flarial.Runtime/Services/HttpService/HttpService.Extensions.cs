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

    internal static Task<string> GetStringAsync(string uri)
    {
        return s_client.GetStringAsync(uri);
    }
}