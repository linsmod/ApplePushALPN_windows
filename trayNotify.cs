using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
public interface INotificationService
{
    Task ShowAsync();
    Task HideAsync();
}

public class TrayNotificationService : INotificationService, IDisposable
{
    private NotifyIcon _notifyIcon;
    private ContextMenuStrip _contextMenu;

    public TrayNotificationService()
    {
        Application.EnableVisualStyles();
        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add("Exit", null, OnExitClick);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.WinLogo,
            Visible = true,
            Text = "Apple推送代理",
            ContextMenuStrip = _contextMenu
        };
    }

    public Task ShowAsync()
    {
        // 在主线程运行 UI 消息循环
        var tcs = new TaskCompletionSource<bool>();

        // 用异步方式启动 UI 线程
        var thread = new Thread(() =>
        {
            tcs.SetResult(true);
            Application.Run();
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return tcs.Task;
    }

    public Task HideAsync()
    {
        _notifyIcon.Visible = false;
        return Task.CompletedTask;
    }

    private void OnExitClick(object sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        Application.Exit();
        Environment.Exit(0);
    }

    public void Dispose()
    {
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
    }
}