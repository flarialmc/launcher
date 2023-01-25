using System.IO;
using System.Net;
using System.Text;

namespace Flarial.Launcher.Functions
{
    public class Auth
    {


        public static string client_id = "1067854754518151168";
        public static string client_sceret = "rfNFSaCf1G5ju34qLaYMQKFrEa4X-a7K";
        public static string redirect_url = "https://flarial.net";

        //https://discord.com/api/oauth2/authorize?client_id=1067854754518151168&redirect_uri=https%3A%2F%2Fflarial.net&response_type=code&scope=guilds%20identify%20guilds.members.read
        public string CodeToToken(string code)
        {

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/oauth2/token%22");
            webRequest.Method = "POST";
            string parameters = "client_id=" + client_id + "&client_secret=" + client_sceret + "&grant_type=authorization_code&code=" + code + "&redirect_uri=" + redirect_url + "";
            byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            Stream postStream = webRequest.GetRequestStream();

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();
            WebResponse response = webRequest.GetResponse();
            postStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(postStream);
            string responseFromServer = reader.ReadToEnd();

            string tokenInfo = responseFromServer.Split(',')[0].Split(':')[1];
            string access_token = tokenInfo.Trim().Substring(1, tokenInfo.Length - 3);

            return access_token;
        }
    }
}
