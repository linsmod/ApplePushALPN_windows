using ConsoleApp2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Push反向代理
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            FormSettingsManager.LoadFormSettings(this);
            FormXmlSerializer.SaveToFile(this, this.Name + "_layout.xml");
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
                this.label3.Text = string.IsNullOrWhiteSpace(this.textBoxTopic.Text) ? "请填写topic" : "";
                if (this.label3.Text.Length > 0)
                {
                    this.textBoxTopic.Focus();
                    return;
                }

                var payload = new JObject(
                    new JProperty("aps", new JObject(
                        new JProperty("alert", new JObject(
                            new JProperty("title", "ttkefu"),
                            new JProperty("body", this.textBox1.Text)
                        )),
                        new JProperty("sound", string.IsNullOrEmpty(comboBox1.Text) ? "default" : comboBox1.Text),
                        new JProperty("badge", 1),
                        new JProperty("content-available", 1)
                    )),
                    new JProperty("MsgType", "msg_type_1"),
                    new JProperty("MsgId", "msg_id_111")
                );

                string jsonPayload = payload.ToString();
                var push = new My_ttkefuPush.PushRequest()
                {
                    Token = this.textBox2.Text,
                    Topic = this.textBoxTopic.Text,
                    Payload = jsonPayload
                };
                await My_ttkefuPush.SendPushNotification(push);
                MessageBox.Show(this, "已发送", "成功");
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
