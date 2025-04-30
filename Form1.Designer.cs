namespace Push反向代理
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            checkBox1 = new CheckBox();
            textBox1 = new TextBox();
            label1 = new Label();
            buttonStart = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            buttonStop = new Button();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItemExit = new ToolStripMenuItem();
            labelErr = new Label();
            label4 = new Label();
            textBoxcert = new TextBox();
            label5 = new Label();
            linkLabel1 = new LinkLabel();
            statusStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(29, 320);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(108, 28);
            checkBox1.TabIndex = 1;
            checkBox1.Text = "开机启动";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Enabled = false;
            textBox1.Location = new Point(29, 218);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(319, 30);
            textBox1.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(29, 191);
            label1.Name = "label1";
            label1.Size = new Size(82, 24);
            label1.TabIndex = 3;
            label1.Text = "监听地址";
            // 
            // buttonStart
            // 
            buttonStart.Location = new Point(216, 320);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(132, 50);
            buttonStart.TabIndex = 9;
            buttonStart.Text = "启动服务";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += button3_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(24, 24);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 503);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(368, 38);
            statusStrip1.TabIndex = 10;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Font = new Font("Microsoft YaHei UI", 12F);
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(206, 31);
            toolStripStatusLabel1.Text = "服务状态：已停止";
            // 
            // buttonStop
            // 
            buttonStop.Location = new Point(216, 394);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(132, 50);
            buttonStop.TabIndex = 9;
            buttonStop.Text = "停止服务";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += button4_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick_1;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripSeparator1, toolStripMenuItemExit });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(117, 40);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(113, 6);
            // 
            // toolStripMenuItemExit
            // 
            toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            toolStripMenuItemExit.Size = new Size(116, 30);
            toolStripMenuItemExit.Text = "退出";
            toolStripMenuItemExit.Click += toolStripMenuItemExit_Click_1;
            // 
            // labelErr
            // 
            labelErr.AutoSize = true;
            labelErr.ForeColor = Color.DodgerBlue;
            labelErr.Location = new Point(12, 479);
            labelErr.Name = "labelErr";
            labelErr.Size = new Size(94, 24);
            labelErr.TabIndex = 11;
            labelErr.Text = "检查配置...";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(29, 105);
            label4.Name = "label4";
            label4.Size = new Size(82, 24);
            label4.TabIndex = 12;
            label4.Text = "证书文件";
            // 
            // textBoxcert
            // 
            textBoxcert.BorderStyle = BorderStyle.FixedSingle;
            textBoxcert.Enabled = false;
            textBoxcert.Location = new Point(29, 132);
            textBoxcert.Name = "textBoxcert";
            textBoxcert.Size = new Size(319, 30);
            textBoxcert.TabIndex = 2;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
            label5.ForeColor = SystemColors.ActiveCaption;
            label5.Location = new Point(27, 24);
            label5.Name = "label5";
            label5.Size = new Size(319, 42);
            label5.TabIndex = 13;
            label5.Text = "ApplePush反向代理";
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(252, 265);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(94, 24);
            linkLabel1.TabIndex = 14;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "推送测试...";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(368, 541);
            Controls.Add(linkLabel1);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(labelErr);
            Controls.Add(statusStrip1);
            Controls.Add(buttonStop);
            Controls.Add(buttonStart);
            Controls.Add(label1);
            Controls.Add(textBoxcert);
            Controls.Add(textBox1);
            Controls.Add(checkBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "ApRelayHost 1.0";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private CheckBox checkBox1;
        private TextBox textBox1;
        private Label label1;
        private TextBox textBox2;
        private Label label2;
        private Label label3;
        private TextBox textBox3;
        private Button button2;
        private Button buttonStart;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button buttonStop;
        private NotifyIcon notifyIcon1;
        private Label labelErr;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItemExit;
        private ToolStripMenuItem toolStripMenuItemRestart;
        private ToolStripMenuItem toolStripMenuItemStop;
        private ToolStripSeparator toolStripSeparator1;
        private Label label4;
        private TextBox textBoxcert;
        private Label label5;
        private LinkLabel linkLabel1;
    }
}