# ApplePushALPN
本项目旨在为缺乏HTTP2/ALPN协议支持的旧版网络堆栈提供代理，比如Windows Server 2016
- `ApplePushALPN`仅运行在Windows，但你可以移植它，`.net`程序已经跨平台很多年了。
- 仅测试`Windows Server 2016`，其他服务器如果支持运行`net8`/`net9`理论上也可以。

# 使用场景
比如你正在旧版Windows上使用`.net framework`，可能无法在TLS2+ALPN上成功交互

![8b35b90d013ca22a0df64c8a77014177](https://github.com/user-attachments/assets/41473b18-c889-465d-8e43-8a8a51e67e87)

`服务器返回的信息无效或不可识别`

这时候ApplePushALPN就该上场了

![image](https://github.com/user-attachments/assets/f15a4102-7127-40e8-954e-212f2539e186)

- 要运行ApplePushALPN，请在开发时使用.net8或者.net9[下载链接](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-9.0.203-windows-x64-installer "sdk-9.0.203-windows-x64-installer")编译
- 为运行它的服务器安装`.net8`或者`.net9`运行时，当然SDK也可以。
- 客户端无此要求

# 代码
客户端代码适配
```
private const string ApiUrl = "http://localhost:5000/api/push";

public static Task<string> SendPushNotification(string token, string msg = "", string issound = "")
{
    try
    {
        var request = new PushRequest
        {
            Token = token,
            Message = msg,
            Sound = string.IsNullOrEmpty(issound) ? "default" : issound
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using (var client = new HttpClient())
        {
            var response = client.PostAsync(ApiUrl, content).Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"状态码: {(int)response.StatusCode} {response.ReasonPhrase}");
            Console.WriteLine($"响应内容: {responseBody}");

            response.EnsureSuccessStatusCode();
            if (responseBody == "OK")
                return Task.FromResult("推送成功！");
            else
                return Task.FromResult(responseBody);
        }
    }
    catch (Exception ex)
    {
        return Task.FromResult("推送失败" + ex.Message);
    }
}

// 请求模型类
public class PushRequest
{
    public string Token { get; set; }
    public string Message { get; set; }
    public string Sound { get; set; }
}
```
