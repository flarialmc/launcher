using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Http.HttpCompletionOption;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    internal static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        return await s_client.SendAsync(request);
    }

    internal static async Task<HttpResponseMessage> GetAsync(string uri, [Optional] CancellationToken token)
    {
        return await s_client.GetAsync(uri, ResponseHeadersRead, token);
    }
}