using System;
using System.Net;
using System.Net.Http;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    static readonly HttpClient s_client = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = true,
        EnableMultipleHttp2Connections = true,
        EnableMultipleHttp3Connections = true,
        AutomaticDecompression = DecompressionMethods.All
    }, true);

    static readonly int s_length = Environment.SystemPageSize;
}