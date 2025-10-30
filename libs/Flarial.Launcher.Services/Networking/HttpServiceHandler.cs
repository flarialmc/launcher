using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using static Windows.Networking.HostNameType;
using static Windows.Networking.Connectivity.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;

namespace Flarial.Launcher.Services.Networking;

sealed partial class HttpServiceHandler : HttpClientHandler
{
    internal static bool UseDnsOverHttps { get; set; }

    internal HttpServiceHandler()
    {
        AllowAutoRedirect = true;
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
    }
}

partial class HttpServiceHandler
{
    static HttpServiceHandler()
    {
        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        s_client = new(handler, true);
        s_client.DefaultRequestHeaders.Add("accept", "application/dns-json");
    }

    static readonly HttpClient s_client;

    const string DnsQueryUri = "https://cloudflare-dns.com/dns-query?name={0}&type={1}";

    static HostNameType? InternetProtocolVersion
    {
        get
        {
            if (GetInternetConnectionProfile() is not { } profile)
                return null;

            HashSet<HostNameType> types = [];

            foreach (var name in GetHostNames())
            {
                if (name.IPInformation is not { } information)
                    continue;

                if (profile.NetworkAdapter.NetworkAdapterId != information.NetworkAdapter.NetworkAdapterId)
                    continue;

                types.Add(name.Type);
            }

            if (types.Contains(Ipv6)) return Ipv6;
            if (types.Contains(Ipv4)) return Ipv4;
            return null;
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        var uri = request.RequestUri;

        if (UseDnsOverHttps && uri.HostNameType is UriHostNameType.Dns && InternetProtocolVersion is { } version)
        {
            var name = uri.Host;

            var type = version switch { Ipv6 => "AAAA", Ipv4 => "A", _ => null };
            var value = version switch { Ipv6 => "28", Ipv4 => "1", _ => null };

            using var stream = await s_client.GetStreamAsync(string.Format(DnsQueryUri, name, type));
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