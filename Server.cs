using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Push反向代理;

namespace ConsoleApp2
{
    internal class Server
    {
        public const string Url = "http://localhost:5000/";
        private bool isMockTest = false;
        private HttpListener listener;
        private CancellationTokenSource _cts;
        private Task _serverTask;
        private readonly object _lock = new();

        public static Server Instance { get; } = new();

        public async Task StartAsync(Form1 mainForm, bool isMockTest)
        {
            lock (_lock)
            {
                if (_serverTask != null && !_cts.IsCancellationRequested)
                    return; // 已经在运行

                _cts = new CancellationTokenSource();
                this.isMockTest = isMockTest;

                _serverTask = RunServerAsync(mainForm, _cts.Token);
            }

            await _serverTask;
        }

        public Task StopAsync()
        {
            _cts?.Cancel();
            return Task.CompletedTask;
        }

        private async Task RunServerAsync(Form1 mainForm, CancellationToken token)
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(Url);
                listener.Start();
                Console.WriteLine($"监听地址：{Url}");
                mainForm.OnServiceStarted();
                while (!token.IsCancellationRequested)
                {
                    HttpListenerContext context = await listener.GetContextAsync().WithCancellation(token);

                    if (context.Request.HttpMethod == "POST" && context.Request.Url?.AbsolutePath == "/api/push")
                    {
                        await HandlePushRequest(context);
                    }
                    else
                    {
                        await WriteResponse(context, "Error, 无效请求");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("服务已取消");
            }
            catch (Exception ex)
            {
                // 回到UI线程更新界面
                mainForm.Invoke((MethodInvoker)delegate
                {
                    mainForm.OnServiceFailed(ex); // 调用窗体方法通知 UI
                });
            }
            finally
            {
                listener?.Stop();

                // 回到UI线程更新界面
                mainForm.Invoke((MethodInvoker)delegate
                {
                    mainForm.OnServiceStopped(); // 调用窗体方法通知 UI
                });
            }
        }

        record PushRequest(string Token, string Message, string? Sound);

        static async Task HandlePushRequest(HttpListenerContext context)
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                string body = await reader.ReadToEndAsync();
                Console.WriteLine("RECV:" + body);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<PushRequest>(body, options);

                if (request == null || string.IsNullOrEmpty(request.Token))
                {
                    await WriteResponse(context, "Error，缺少Token参数");
                    return;
                }

                if (Instance.isMockTest)
                {
                    await WriteResponse(context, "OK, 本地测试模式未真实推送");
                    return;
                }

                var result = await My_ttkefuPush.SendPushNotification(
                     request.Token,
                     request.Message,
                     request.Sound ?? "default");

                await WriteResponse(context, result);
            }
            catch (Exception ex)
            {
                await WriteResponse(context, $"Error: {ex.Message}");
            }
        }

        static async Task WriteResponse(HttpListenerContext context, string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("回写HTTP响应失败：" + ex.Message);
            }
        }
    }
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            }
            return await task;
        }
    }
}