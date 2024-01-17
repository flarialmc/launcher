using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Flarial.Launcher.Functions
{
    public class TokenStructure
    {
        public string access_token { get; set; }

        public DateTime created { get; set; }
        public DateTime expiry { get; set; }

    }
    public static class Auth
    {
        public static string Path = $"{Managers.VersionManagement.launcherPath}\\cachedToken.txt";
        public static string client_id = "1127566162490835006";
        public static string client_sceret = "vMVoSCw3DciGp0p0HA7H6RjmMHWOXdIV";
        public static string redirect_url = "https://flarial.net";

        public static string postReq(string code)
        {
            var client = new RestClient("https://discord.com");
            var request = new RestRequest("api/oauth2/token");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", client_id);
            request.AddParameter("client_secret", client_sceret);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", redirect_url);
            request.AddParameter("scope", "identify guild.members.read guilds");

            var response = client.PostAsync(request);
            return response.Result.Content;
        }

        public static async Task<string> getReqUser(string authCode)
        {
            var client = new RestClient("https://discord.com");
            var request = new RestRequest("api/v10/users/@me");
            request.AddHeader("Authorization", "Bearer " + authCode);
            var response = await client.GetAsync(request);
            return response.Content;
        }

        public static async Task<string> getReqGuildUser(string authCode)
        {
            var client = new RestClient("https://discord.com");
            var request = new RestRequest("api/v10/users/@me/guilds/1049946152092586054/member");
            request.AddHeader("Authorization", "Bearer " + authCode);
            var response = await client.GetAsync(request);
            return response.Content;
        }

        public static async Task CacheToken(string Token, DateTime Created, DateTime Expiry)
        {
            if (!File.Exists(Path))
            {
                await Task.Run(() => File.WriteAllText(Path, string.Empty));
            }

            var raw = await Task.Run(() => File.ReadAllText(Path));

            if (string.IsNullOrEmpty(raw))
            {
                var ts = new TokenStructure()
                {
                    access_token = Token,
                    created = Created,
                    expiry = Expiry
                };

                var tss = JsonConvert.SerializeObject(ts);

                await Task.Run(() => File.WriteAllText(Path, tss));
            }
            else
            {
                try
                {
                    var decoded = JsonConvert.DeserializeObject<TokenStructure>(raw);
                    if (decoded != null && DateTime.Now < decoded.expiry)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    return;
                }

                File.Delete(Path);

                var ts = new TokenStructure()
                {
                    access_token = Token,
                    created = Created,
                    expiry = Expiry
                };

                var tss = JsonConvert.SerializeObject(ts);

                await Task.Run(() => File.WriteAllText(Path, tss));
            }
        }

        public static async Task<TokenStructure> GetCache()
        {
            if (!File.Exists(Path))
            {
                return null;
            }

            var s = await Task.Run(() => File.ReadAllText(Path));
            return JsonConvert.DeserializeObject<TokenStructure>(s);
        }
    }
}
