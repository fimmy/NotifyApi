using Flurl.Http;
using System.Dynamic;
using System.Text.Json;

namespace NotifyApi
{
    public class AccessToken
    {
        IConfiguration configuration;
        string weComId;
        string weSecret;
        private readonly string tokenFilePath = "token.json";
        public AccessToken(IConfiguration configuration)
        {
            this.configuration = configuration;
            weComId = configuration["WeComId"];
            weSecret = configuration["WeSecret"];
            try
            {
                if (File.Exists(tokenFilePath))
                {
                    var json = File.ReadAllText(tokenFilePath);
                    var doc=JsonDocument.Parse(json);
                    token = doc.RootElement.GetProperty("token").GetString()??"";
                    expiresDatetime = doc.RootElement.GetProperty("expiresDatetime").GetDateTime();      
                }

            }
            catch (Exception ex)
            {
                token = "";
                expiresDatetime = DateTime.Now;
            }


        }
        private DateTime expiresDatetime;
        private string token="";
        public async Task<string> GetToken()
        {

            if (DateTime.Now > expiresDatetime||string.IsNullOrEmpty(token))
            {
                var now = DateTime.Now;
                var res = await $"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={weComId}&corpsecret={weSecret}".GetJsonAsync();
                var errcode = res.errcode;
                if (errcode == 0)
                {
                    token = res.access_token;
                    expiresDatetime = now.AddSeconds(res.expires_in);
                }
                else
                {
                    token = "";
                    expiresDatetime = now;
                }
                await writeToFile();
            }
            return token;
        }
        private async Task writeToFile()
        {
            using (FileStream createStream = File.OpenWrite(tokenFilePath))
            {
                await JsonSerializer.SerializeAsync(createStream, new {token=token,expiresDatetime=expiresDatetime});
            }
        }

    }
}
