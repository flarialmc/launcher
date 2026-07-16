using System.Buffers.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Flarial.Runtime.Identity;

static class RequestHelper
{
   internal static string GetRedirectUri()
    {
        using TcpListener listener = new(IPAddress.Loopback, 0);

        listener.Start();
        try
        {
            var endpoint = (IPEndPoint)listener.LocalEndpoint;

            var port = endpoint.Port;
            var address = endpoint.Address;

            return $"http://{address}:{port}/oauth/callback";
        }
        finally
        {
            listener.Stop();
        }
    }

   internal static (string Verifier, string Challenge) CreateCodeExchange()
    {
        var rng = RandomNumberGenerator.GetBytes(32);

        var verifier = Base64Url.EncodeToString(rng);
        var utf8 = Encoding.UTF8.GetBytes(verifier);

        var data = SHA256.HashData(utf8);
        var challenge = Base64Url.EncodeToString(data);

        return (verifier, challenge);
    }
}