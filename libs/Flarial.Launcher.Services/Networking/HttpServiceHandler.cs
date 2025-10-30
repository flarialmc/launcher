using System;
using static System.Net.DecompressionMethods;
using static System.Net.IPAddress;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using static Windows.Networking.HostNameType;
using static System.UriHostNameType;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Http.HttpCompletionOption;
using System.Linq;
using System.Net.Sockets;

namespace Flarial.Launcher.Services.Networking;

sealed partial class HttpServiceHandler : HttpClientHandler
{
    internal static bool UseDnsOverHttps { get; set; }

    internal HttpServiceHandler()
    {
        AllowAutoRedirect = true;
        AutomaticDecompression = GZip | Deflate;
    }

    static HttpServiceHandler()
    {
        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = GZip | Deflate
        };

        s_client = new(handler, true);
        s_client.DefaultRequestHeaders.Add("accept", "application/dns-json");
    }

    static readonly HttpClient s_client;

    const string TraceUri = "https://speed.cloudflare.com/__down";

    const string DnsUri = "https://cloudflare-dns.com/dns-query?name={0}&type={1}";

    static async Task<HostNameType?> VersionAsync()
    {
        try
        {
            using var message = await s_client.GetAsync(TraceUri, ResponseHeadersRead);
            var @string = message.Headers.GetValues("cf-meta-ip").FirstOrDefault();

            if (!TryParse(@string, out var address))
                return null;

            return address.AddressFamily switch
            {
                AddressFamily.InterNetwork => Ipv4,
                AddressFamily.InterNetworkV6 => Ipv6,
                _ => null
            };
        }
        catch (HttpRequestException) { return null; }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        var uri = request.RequestUri;

        if (UseDnsOverHttps && uri.HostNameType is Dns && await VersionAsync() is { } version)
        {
            var name = uri.Host;

            var type = version switch { Ipv6 => "AAAA", Ipv4 => "A", _ => null };
            var value = version switch { Ipv6 => "28", Ipv4 => "1", _ => null };

            using var stream = await s_client.GetStreamAsync(string.Format(DnsUri, name, type));
            using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);

            foreach (var element in XElement.Load(reader).Descendants("data"))
                if (element.Parent.Element("type").Value == value)
                {
                    UriBuilder builder = new(uri) { Host = element.Value };
                    request.RequestUri = builder.Uri; request.Headers.Host = name;
                    break;
                }
        }

        return await base.SendAsync(request, token);
    }
}