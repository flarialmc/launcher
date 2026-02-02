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

public sealed class DnsOverHttpsHandler : HttpClientHandler
{
    public static bool? UseDnsOverHttps
    {
        internal get;
        set { field ??= value; }
    }

    internal DnsOverHttpsHandler()
    {
        AllowAutoRedirect = true;
        AutomaticDecompression = GZip | Deflate;
    }

    static DnsOverHttpsHandler() => s_client.DefaultRequestHeaders.Add("accept", "application/dns-json");

    static readonly HttpClient s_client = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
        AutomaticDecompression = GZip | Deflate
    }, true);

    const string AddressMetadataUrl = "https://speed.cloudflare.com/__down";

    const string DnsQueryUrl = "https://cloudflare-dns.com/dns-query?name={0}&type={1}";

    static async Task<HostNameType?> GetProtocolAsync()
    {
        try
        {
            using var message = await s_client.GetAsync(AddressMetadataUrl, ResponseHeadersRead);
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

        if ((UseDnsOverHttps ??= false) && uri.HostNameType is Dns && await GetProtocolAsync() is { } protocol)
        {
            var name = uri.Host;

            var type = protocol switch
            {
                Ipv6 => "AAAA",
                Ipv4 => "A",
                _ => throw new InvalidOperationException()
            };

            var value = protocol switch
            {
                Ipv6 => "28",
                Ipv4 => "1",
                _ => throw new InvalidOperationException()
            };

            using var stream = await s_client.GetStreamAsync(string.Format(DnsQueryUrl, name, type));
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