using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timer = System.Windows.Forms.Timer;

internal class FormSettingsManager
{
    private static JObject _userSettings = new JObject();
    private static readonly string _filePath = "form.json";
    // 新增：延迟保存的 Timer
    private static Timer _saveDelayTimer;
    static FormSettingsManager()
    {
        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            _userSettings = JObject.Parse(json);
        }
        // 初始化定时器
        _saveDelayTimer = new Timer { Interval = 300 }; // 300ms
        _saveDelayTimer.Tick += (s, e) =>
        {
            _saveDelayTimer.Stop(); // 停止定时器
            PerformSave(); // 执行真正的保存逻辑
        };
    }

    public static void LoadFormSettings(Form form)
    {
        string formKey = "form_" + form.Name;

        if (!_userSettings.ContainsKey(formKey))
        {
            _userSettings[formKey] = new JObject
            {
                ["controls"] = new JObject()
            };
        }

        var controlSettings = (JObject)_userSettings[formKey]["controls"];

        foreach (Control control in form.Controls)
        {
            if (control is CheckBox checkBox)
            {
                ApplyCheckBoxSetting(checkBox, controlSettings);
            }
            else if (control is TextBox textBox)
            {
                ApplyTextBoxSetting(textBox, controlSettings);
            }
            else if (control is ComboBox comboBox)
            {
                ApplyComboBoxSetting(comboBox, controlSettings);
            }

            // 可继续添加其他控件支持
        }
    }

    static void ApplyCheckBoxSetting(CheckBox cb, JObject controlSettings)
    {
        var key = cb.Name;
        if (controlSettings.ContainsKey(key) && controlSettings[key].HasValues)
        {
            cb.Checked = (bool)controlSettings[key]["checked"];
        }

        controlSettings[key] = new JObject
        {
            ["checked"] = cb.Checked
        };

        cb.Tag = controlSettings[key];

        cb.CheckedChanged += (s, e) =>
        {
            var data = (JObject)cb.Tag;
            data["checked"] = cb.Checked;
            SaveSettings();
        };
    }

    static void ApplyTextBoxSetting(TextBox tb, JObject controlSettings)
    {
        var key = tb.Name;
        if (controlSettings.ContainsKey(key) && controlSettings[key].HasValues)
        {
            tb.Text = (string)controlSettings[key]["text"];
        }

        controlSettings[key] = new JObject
        {
            ["text"] = tb.Text,
            ["borderStyle"] = tb.BorderStyle.ToString(),
        };

        tb.Tag = controlSettings[key];

        tb.TextChanged += (s, e) =>
        {
            var data = (JObject)tb.Tag;
            data["text"] = tb.Text;
            SaveSettings();
        };
    }

    static void ApplyComboBoxSetting(ComboBox cmb, JObject controlSettings)
    {
        var key = cmb.Name;
        if (controlSettings.ContainsKey(key) && controlSettings[key].HasValues)
        {
            cmb.Text = (string)controlSettings[key]["text"];
        }

        controlSettings[key] = new JObject
        {
            ["text"] = cmb.Text
        };

        cmb.Tag = controlSettings[key];

        cmb.TextChanged += (s, e) =>
        {
            var data = (JObject)cmb.Tag;
            data["text"] = cmb.Text;
            SaveSettings();
        };
    }

    // 公共触发保存的方法（会重启定时器）
    public static void SaveSettings()
    {
        _saveDelayTimer.Stop(); // 如果已有倒计时，则重置
        _saveDelayTimer.Start(); // 重新开始300ms倒计时
    }

    // 真正执行保存的方法
    private static void PerformSave()
    {
        try
        {
            File.WriteAllText(_filePath, _userSettings.ToString(Formatting.Indented));
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存设置失败：" + ex.Message);
        }
    }
}