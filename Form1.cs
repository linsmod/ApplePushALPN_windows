using ConsoleApp2;
using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
using File = System.IO.File;

namespace Push反向代理
{
    public partial class Form1 : Form
    {
        private bool exitRequested;
        private string shortLinkName = "ApplePush反向代理";
        public Form1()
        {
            InitializeComponent();
            FormXmlSerializer.SaveToFile(this, this.Name + "_layout.xml");
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var certFile = checkCert();
            if (certFile == null) { return; }
            try
            {
                this.toolStripStatusLabel1.Text = "正在启动...";
                this.toolStripStatusLabel1.ForeColor = Color.Green;
                this.buttonStart.Enabled = false;
                this.buttonStop.Enabled = true;
                My_ttkefuPush.certFile = certFile;
                await Server.Instance.StartAsync(this, false);
            }
            catch (Exception ex)
            {
                OnServiceFailed(ex);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            try
            {
                await Server.Instance.StopAsync();
                OnServiceStopped();
            }
            catch (Exception ex)
            {
                OnServiceFailed(ex);
            }
        }

        internal void OnServiceStarted()
        {
            this.toolStripStatusLabel1.Text = "服务状态：运行中";
            this.toolStripStatusLabel1.ForeColor = Color.DarkGreen;
            this.labelErr.Text = "";
        }
        internal void OnServiceStopped()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(OnServiceStopped));
                return;
            }

            this.toolStripStatusLabel1.Text = "服务状态：已停止";
            this.toolStripStatusLabel1.ForeColor = Color.DarkRed;
            this.buttonStart.Enabled = true;
            this.buttonStop.Enabled = false;
        }

        internal void OnServiceFailed(Exception ex)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<Exception>(OnServiceFailed), ex);
                return;
            }

            this.toolStripStatusLabel1.Text = "服务状态：已停止（发生错误）";
            this.toolStripStatusLabel1.ForeColor = Color.Red;
            this.labelErr.Text = "错误消息：" + ex.Message;
            this.labelErr.ForeColor = Color.Red;

            this.buttonStart.Enabled = true;
            this.buttonStop.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            this.notifyIcon1.Text = this.Text;
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupPath, shortLinkName + ".lnk");

            checkBox1.Checked = File.Exists(shortcutPath);
            checkBox2.Checked = File.Exists(logFlagFile);
            this.textBox1.Text = Server.Url;
            textBoxLog.ReadOnly = true;
            Server.Instance.LogChanged += Instance_LogChanged;
            button3_Click(null, null);

        }

        private void Instance_LogChanged(object sender, string e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateLogDisplay(e)));
            }
            else
            {
                UpdateLogDisplay(e);
            }
        }

        private void UpdateLogDisplay(string newLogEntry)
        {
            // 如果当前行数超过限制，移除第一行
            if (textBoxLog.Lines.Length >= 100)
            {
                // 找到第一个换行符的位置
                int firstLineEnd = textBoxLog.Text.IndexOf('\n');
                if (firstLineEnd != -1)
                {
                    textBoxLog.Text = textBoxLog.Text.Substring(firstLineEnd + 1);
                }
            }

            // 添加新日志
            textBoxLog.AppendText(newLogEntry + Environment.NewLine);

            // 自动滚动到底部
            textBoxLog.ScrollToCaret();
        }

        private string checkCert()
        {
            this.labelErr.Text = "";
            try
            {
                // 获取当前运行目录下的所有 .p12 和 .pfx 文件
                string[] certFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*")
                    .Where(f => f.EndsWith(".p12", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (certFiles.Length == 0)
                {
                    this.labelErr.Text = "配置错误：缺少证书文件 (.p12 或 .pfx)";
                    this.labelErr.ForeColor = Color.Red;
                    return null;
                }

                string certFilePath = certFiles[0]; // 使用第一个匹配的证书文件
                string certPassword = Path.GetFileNameWithoutExtension(certFilePath); // 你可以考虑从配置或输入框获取这个密码

                X509Certificate2 certificate = new X509Certificate2(certFilePath, certPassword, X509KeyStorageFlags.Exportable);

                if (!certificate.HasPrivateKey)
                {
                    this.labelErr.Text = "配置错误：证书不包含私钥，无法使用";
                    this.labelErr.ForeColor = Color.Red;
                    return null;
                }

                this.textBoxcert.Text = $"{Path.GetFileName(certFilePath)}";
                return certFilePath; // 返回证书路径供后续使用
            }
            catch (Exception ex)
            {
                this.textBoxcert.Text = $"证书错误：{ex.Message}";
                this.labelErr.ForeColor = Color.Red;
                return null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.exitRequested)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string shortcutPath = GetShortcutPath();
            string exePath = Application.ExecutablePath;

            if (checkBox1.Checked)
            {
                CreateShortcut(shortcutPath, exePath, null, "启动 ApplePush反向代理");
            }
            else
            {
                if (File.Exists(shortcutPath))
                {
                    try
                    {
                        File.Delete(shortcutPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("删除开机启动快捷方式失败：" + ex.Message);
                    }
                }
            }
        }

        private string GetStartupFolderPath() =>
            Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        private string GetShortcutPath() =>
            Path.Combine(GetStartupFolderPath(), shortLinkName + ".lnk");

        private void CreateShortcut(string shortcutPath, string targetPath, string arguments, string description)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.Arguments = arguments;
                shortcut.Description = description;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法设置开机启动：" + ex.Message);
                checkBox1.Checked = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {

            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new Form2().ShowDialog();
        }

        private void toolStripMenuItemExit_Click_1(object sender, EventArgs e)
        {
            exitRequested = true;
            Application.Exit();
        }
        string logFlagFile = "log.enabled";

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox2.Checked)
            {
                // 如果勾选，创建文件（如果不存在）
                File.WriteAllText(logFlagFile, "删除本文件可以禁止输出日志");
            }
            else
            {
                // 如果取消勾选，删除文件（如果存在）
                try
                {
                    if (File.Exists(logFlagFile))
                    {
                        File.Delete(logFlagFile);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("无法删除日志标记文件：" + ex.Message);
                }
            }
            Server.Instance.LogEnabled = checkBox2.Checked;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

            try
            {
                if (File.Exists(logFilePath))
                {
                    // 使用 ProcessStartInfo 提高兼容性，并避免自动寻找关联程序失败的情况
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = logFilePath,
                        UseShellExecute = true // 让系统决定如何打开
                    });
                }
                else
                {
                    MessageBox.Show("现在还没有任何记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开日志文件：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}