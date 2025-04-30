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
    public static async Task<string> SendPushNotification(PushRequest push)
    {
        if (push.Token == null)
        {
            return "错误：缺少Token参数";
        }
        if (push.Topic == null)
        {
            return "错误：缺少Topic参数";
        }
        // 加载 p12 证书
        if (!File.Exists(certFile))
        {
            return "反向代理的运行目录里缺少证书文件";
        }
        string p12Password = Path.GetFileNameWithoutExtension(certFile);
        X509Certificate2 certificate = new X509Certificate2(certFile, p12Password, X509KeyStorageFlags.Exportable);

        if (!certificate.HasPrivateKey)
        {
            return "为反向代理配置证书没有私钥，无法使用。";
        }

        // APNs 设置
        string apnsEndpoint = "https://api.push.apple.com";
        string topic = push.Topic;

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
        var content = new StringContent(push.Payload, Encoding.UTF8, "application/json");

        // 构建请求
        var request = new HttpRequestMessage(HttpMethod.Post, $"{apnsEndpoint}/3/device/{push.Token}")
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
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return "OK";
        }

        else
        {
            return $"推送失败：苹果返回{response.StatusCode},{responseBody}";
        }
    }
    public class PushRequest
    {
        public string Topic { get; set; }
        public string Token { get; set; }
        public string Payload { get; set; }
    }
}