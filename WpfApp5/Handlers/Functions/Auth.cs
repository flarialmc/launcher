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

        public static string client_id = "1067854754518151168";
        public static string client_sceret = "rfNFSaCf1G5ju34qLaYMQKFrEa4X-a7K";
        public static string redirect_url = "https://flarial.net";



        //https://discord.com/api/oauth2/authorize?client_id=1067854754518151168&redirect_uri=https%3A%2F%2Fflarial.net&response_type=code&scope=guilds%20identify%20guilds.members.read
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

            var response = client.Post(request);
            var content = response.Content;
            return content;

        }

        public static string getReqUser(string authCode)
        {

            var client = new RestClient("https://discord.com");
            var request = new RestRequest("api/v10/users/@me");
            request.AddHeader("Authorization", "Bearer " + authCode);
            var response = client.Get(request);
            return response.Content;
        }

        public static async Task cacheToken(string Token, DateTime Created, DateTime Expiry)
        {
            if (!File.Exists(Path))
            {
                File.Create(Path);


                await Task.Delay(1000);

            }


            var raw = File.ReadAllText(Path);

            if (raw.Length == 0)
            {
                var ts = new TokenStructure()
                {
                    access_token = Token,
                    created = Created,
                    expiry = Expiry
                };

                var tss = JsonConvert.SerializeObject(ts);

                File.WriteAllText(Path, tss);
                return;

            }
            else
            {
                try
                {
                    var Decoded = JsonConvert.DeserializeObject<TokenStructure>(raw);
                    if (Decoded == null)
                    {
                        return;
                    }
                    else
                    {
                        if (DateTime.Now >= Decoded.expiry)
                        {
                            File.Delete(Path);
                            var ts = new TokenStructure()
                            {
                                access_token = Token,
                                created = Created,
                                expiry = Expiry
                            };

                            var tss = JsonConvert.SerializeObject(ts);

                            File.WriteAllText(Path, tss);
                            return;

                        }
                        else
                        {
                            return;

                        }
                    }
                }
                catch (Exception ex)
                {
                    return;
                }


            }



        }

        public static async Task<TokenStructure>? getCache()
        {
            if (!File.Exists(Path))
            {
                return null;
            }
            var s = File.ReadAllText(Path);


            return JsonConvert.DeserializeObject<TokenStructure>(s);

        }

        //private void putjoinuser(AccessTokenData authcode, string userid)
        //{

        //    var client = new RestClient("https://discord.com");
        //    var request = new RestRequest("api/v10/guilds/1049946152092586054/members/" + userid);
        //    request.AddHeader("Authorization", "Bot MTA1ODQyNjk2NjYwMjE3NDQ3NA.GAno-1.uYVeAJVBflLNpjKt0jCtxnXn7CJwyOdoQEK3NY");
        //    request.AddHeader("Content-Type", "application/json");
        //    request.AddBody(JsonConvert.SerializeObject(authcode));
        //    client.Put(request);
        //}
    }
}
