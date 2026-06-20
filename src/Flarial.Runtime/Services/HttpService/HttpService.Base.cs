using System;
using System.Net;
using System.Net.Http;

namespace Flarial.Runtime.Services;

static partial class HttpService
{
    static readonly HttpClient s_client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = int.MaxValue,
        AutomaticDecompression = DecompressionMethods.All
    }, true);

    static readonly int s_length = Environment.SystemPageSize;
}