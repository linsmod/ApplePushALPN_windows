using ConsoleApp2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Push反向代理;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();
    private static Mutex _mutex = null;
    public static async Task Main(string[] args)
    {
        if (Environment.OSVersion.Version.Major >= 6)
        {
            SetProcessDPIAware();
        }
        // 使用唯一名称创建一个系统级互斥量（名称任意，但必须唯一）
        string appName = "Push反向代理-UniqueInstanceMutex";
        bool createdNew;

        try
        {
            _mutex = new Mutex(true, appName, out createdNew);
        }
        catch (Exception ex)
        {
            MessageBox.Show("无法创建互斥对象：" + ex.Message);
            return;
        }

        // 如果没有创建成功，说明已有实例在运行
        if (!createdNew)
        {
            MessageBox.Show("应用已在运行中。");
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());
        //bool isTestMode = args.Length > 0 && args[0].EndsWith("test");
        //isTestMode = true;
        //var hostBuilder = Host.CreateDefaultBuilder(args);

        //if (!isTestMode)
        //{
        //    hostBuilder.UseWindowsService(options =>
        //    {
        //        options.ServiceName = "My HTTP Push Service";
        //    });
        //}
        //else
        //{
        //    Console.WriteLine("运行在测试模式（控制台 + 托盘图标）");
        //}

        //hostBuilder.ConfigureServices((context, services) =>
        //{
        //    services.AddSingleton<INotificationService, TrayNotificationService>();
        //    services.AddHostedService<Server>();
        //});

        //var host = hostBuilder.Build();
        //await RunWithTrayAsync(host);
        //if (isTestMode)
        //{
        //    // 托盘图标需要在主线程创建 UI 组件
        //    await RunWithTrayAsync(host);
        //}
        //else
        //{
        //    await host.RunAsync();
        //}
    }

    private static async Task RunWithTrayAsync(IHost host)
    {
        var notifyService = host.Services.GetRequiredService<INotificationService>();
        await notifyService.ShowAsync();

        await host.RunAsync();
    }
}