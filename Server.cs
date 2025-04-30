using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Push反向代理;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static My_ttkefuPush;

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
        public bool LogEnabled { get; internal set; }

        private static readonly SemaphoreSlim LogLock = new SemaphoreSlim(1, 1); // 异步锁
        public event EventHandler<string> LogChanged;

        protected virtual void OnLogChanged(string e)
        {
            try
            {
                LogChanged?.BeginInvoke(this, e,null,null);
            }
            catch (Exception ex) { }
        }

        private async Task LogRequestAsync(string logMessage)
        {
            if (!LogEnabled)
                return;
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

            try
            {
                await LogLock.WaitAsync();

                try
                {
                    var newLogEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}";
                    // 触发事件
                    OnLogChanged(newLogEntry);

                    using (var writer = new StreamWriter(logFilePath, true))
                    {
                        await writer.WriteLineAsync($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}");
                    }
                }
                finally
                {
                    LogLock.Release();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"无法写入日志文件: {ex.Message}");
            }
        }

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
                await LogRequestAsync($"服务已启动，监听地址：{Url}");
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
                        await LogRequestAsync($"收到无效请求: {context.Request.HttpMethod} {context.Request.Url}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //Console.WriteLine("服务已取消");
                //await LogRequestAsync("服务已取消");
            }
            catch (Exception ex)
            {
                // 回到UI线程更新界面
                mainForm.Invoke((MethodInvoker)delegate
                {
                    mainForm.OnServiceFailed(ex); // 调用窗体方法通知 UI
                });
                await LogRequestAsync($"服务异常: {ex.Message}");
            }
            finally
            {
                listener?.Stop();

                // 回到UI线程更新界面
                mainForm.Invoke((MethodInvoker)delegate
                {
                    mainForm.OnServiceStopped(); // 调用窗体方法通知 UI
                });
                await LogRequestAsync("服务已停止");
            }
        }

        static async Task HandlePushRequest(HttpListenerContext context)
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                string body = await reader.ReadToEndAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<PushRequest>(body, options);
                await Instance.LogRequestAsync($"收到POST请求: {body}");
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

                var result = await SendPushNotification(
                     request);

                await WriteResponse(context, result);
            }
            catch (Exception ex)
            {
                await WriteResponse(context, $"Error: {ex.Message}");
                await Instance.LogRequestAsync($"Error: {ex.Message}");
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
                await Instance.LogRequestAsync($"响应内容: {message}");
            }
            catch (Exception ex)
            {
                await Instance.LogRequestAsync($"响应失败: {ex.Message}");
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