using ConsoleApp2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Push反向代理
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            this.label3.TextChanged += Label3_TextChanged;
        }

        private void Label3_TextChanged(object sender, EventArgs e)
        {
            if (label3.Text.Length > 0)
                label3.ForeColor = Color.Red;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            this.button1.Text = "发送中";
            this.button1.Enabled = false;
            this.label3.Text = "";
            try
            {
                this.label3.Text = string.IsNullOrWhiteSpace(this.textBox1.Text) ? "请填写消息内容" : "";
                if (this.label3.Text.Length > 0)
                {
                    this.textBox1.Focus();
                    this.textBox2.PlaceholderText = "必填";
                    return;
                }
                this.label3.Text = string.IsNullOrWhiteSpace(this.textBox2.Text) ? "请填写Token" : "";
                if (this.label3.Text.Length > 0)
                {
                    this.textBox2.Focus();
                    this.textBox2.PlaceholderText = "必填";
                    return;
                }
                await My_ttkefuPush.SendPushNotification(this.textBox2.Text, this.textBox1.Text, this.comboBox1.Text);
            }
            catch (HttpRequestException ex)
            {
                var sockeError = ex.InnerException as SocketException;
                if (sockeError != null)
                {
                    MessageBox.Show(this, "网络错误\n" + ex.Message, "网络错误");
                }
                else
                {
                    MessageBox.Show(this, ex.ToString(), "错误");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "错误");
            }
            finally
            {
                this.button1.Text = "发送";
                this.button1.Enabled = true;
            }
        }
    }
}
