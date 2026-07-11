using System.Net.Http;
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

    internal static Task<HttpResponseMessage> PostAsync(string uri, HttpContent content)
    {
        return s_client.PostAsync(uri, content);
    }

    static Task<HttpResponseMessage> GetAsync(string uri, CancellationToken token)
    {
        return s_client.GetAsync(uri, ResponseHeadersRead, token);
    }
}