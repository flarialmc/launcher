using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Http.HttpCompletionOption;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    internal static Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        return s_client.SendAsync(request);
    }

    static Task<HttpResponseMessage> GetAsync(string uri, [Optional] CancellationToken token)
    {
        return s_client.GetAsync(uri, ResponseHeadersRead, token);
    }
}