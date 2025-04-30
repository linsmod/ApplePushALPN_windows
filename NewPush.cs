using System;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class My_ttkefuPush
{
    internal static string certFile;

    public static async Task<string> SendPushNotification(string token, string msg = "", string issound = "")
    {
        // 加载 p12 证书
        if (!File.Exists(certFile))
        {
            return "反向代理的运行目录里缺少证书文件";
        }
        string p12Password = "1234";
        X509Certificate2 certificate = new X509Certificate2(certFile, p12Password, X509KeyStorageFlags.Exportable);

        if (!certificate.HasPrivateKey)
        {
            return "为反向代理配置证书没有私钥，无法使用。";
        }

        // APNs 设置
        string apnsEndpoint = "https://api.push.apple.com";
        string topic = "kefuSystem.production.IMClient";

        var payload = new
        {
            aps = new
            {
                alert = new
                {
                    title = "kefuSystem",
                    body = msg
                },
                sound = issound,
                badge = 1
            }
        };

        // 构建自定义 Handler
        var handler = new SocketsHttpHandler
        {
            SslOptions =
            {
                // 指定 TLS 版本
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,

                // 启用 ALPN h2 协议
                ApplicationProtocols = new List<System.Net.Security.SslApplicationProtocol> { System.Net.Security.SslApplicationProtocol.Http2 },

                // 忽略证书验证（测试环境可用）
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true,

                // 添加客户端证书
                ClientCertificates = new X509CertificateCollection { certificate }
            },
            ConnectTimeout = TimeSpan.FromSeconds(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            EnableMultipleHttp2Connections = true
        };

        // 创建 HttpClient 实例
        using var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        // 构建 JSON 请求体
        string jsonString = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        // 构建请求
        var request = new HttpRequestMessage(HttpMethod.Post, $"{apnsEndpoint}/3/device/{token}")
        {
            Version = new Version(2, 0), // 明确指定 HTTP/2
            Content = content
        };

        // 设置 APNs 必要头信息
        request.Headers.Add("apns-topic", topic);
        request.Headers.Add("apns-push-type", "alert");

        // 发送请求
        HttpResponseMessage response = await client.SendAsync(request);

        // 输出响应
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"状态码: {(int)response.StatusCode} {response.ReasonPhrase}");
        Console.WriteLine($"响应内容: {responseBody}");

        // 确保成功
        response.EnsureSuccessStatusCode();

        return "推送成功！";
    }
}