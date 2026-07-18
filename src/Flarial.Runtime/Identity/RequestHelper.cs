using System;
using System.Buffers.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using static System.Net.IPAddress;
using static System.Net.Sockets.AddressFamily;
using static System.Net.Sockets.ProtocolType;
using static System.Net.Sockets.SocketType;

namespace Flarial.Runtime.Identity;

static class RequestHelper
{
    static readonly IPEndPoint s_endpoint = new(Loopback, 0);

    internal static string CreateRedirectUri()
    {
        using Socket socket = new(InterNetwork, Stream, Tcp);
        socket.Bind(s_endpoint);

        if (socket.LocalEndPoint is not IPEndPoint endpoint)
            throw new InvalidOperationException();

        var port = endpoint.Port;
        var address = endpoint.Address;

        return $"http://{address}:{port}/oauth/callback";
    }

    internal static (string CodeVerifier, string CodeChallenge) CreateCodeExchange()
    {
        var rng = RandomNumberGenerator.GetBytes(32);

        var verifier = Base64Url.EncodeToString(rng);
        var utf8 = Encoding.UTF8.GetBytes(verifier);

        var data = SHA256.HashData(utf8);
        var challenge = Base64Url.EncodeToString(data);

        return (verifier, challenge);
    }

    internal static string CreateApplicationState()
    {
        var rng = RandomNumberGenerator.GetBytes(32);
        return Base64Url.EncodeToString(SHA256.HashData(rng));
    }
}