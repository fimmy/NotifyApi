using NotifyApi;
using Flurl.Http;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<AccessToken>();
var app = builder.Build();
// Configure the HTTP request pipeline.

app.MapGet("/send", async (string clientId,string text, string toUser,AccessToken accessToken,IConfiguration configuration) =>
{
    var clientIds=configuration.GetSection("clientIds").Get<string[]>();
    var agentid = configuration.GetValue<string>("WeAgentId");
    if (clientIds.Contains(clientId))
    {
        var token = await accessToken.GetToken();
        var resp = await $"https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={token}".PostJsonAsync(new
        {
            touser = toUser,
            agentid = agentid,
            msgtype = "text",
            text = new
            {
                content = text
            },
            duplicate_check_interval = 600
        });
        return await resp.ResponseMessage.Content.ReadAsStringAsync();
    }
    else
    {
        return "非法客户端";
    }
});

app.Run();
