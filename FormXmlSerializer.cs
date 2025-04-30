using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

public static class FormXmlSerializer
{
    public static XElement Serialize(Form form)
    {
        var formElement = new XElement("Form",
            new XAttribute("Name", form.Name),
            new XAttribute("Text", form.Text),
            new XAttribute("Size", $"{form.Width},{form.Height}"),
            new XAttribute("StartPosition", form.StartPosition.ToString())
        );

        foreach (Control control in GetAllControls(form))
        {
            formElement.Add(SerializeControl(control));
        }

        return formElement;
    }

    private static XElement SerializeControl(Control control)
    {
        var element = new XElement("Control",
            new XAttribute("Name", control.Name ?? ""),
            new XAttribute("Type", control.GetType().AssemblyQualifiedName),
            new XAttribute("Text", control.Text),
            new XAttribute("Location", $"{control.Left},{control.Top}"),
            new XAttribute("Size", $"{control.Width},{control.Height}"),
            new XAttribute("Anchor", control.Anchor.ToString()),
            new XAttribute("Dock", control.Dock.ToString()),
            new XAttribute("Enabled", control.Enabled),
            new XAttribute("ForeColor", ColorToString(control.ForeColor)),
            new XAttribute("BackColor", ColorToString(control.BackColor)),
            new XAttribute("Font", FontToString(control.Font))
        );

        //// 支持枚举类型如 AnchorStyles
        //var anchorType = typeof(AnchorStyles);
        //if (control.Anchor != (AnchorStyles)0)
        //{
        //    var enumEl = new XElement("Enum",
        //        new XAttribute("Type", anchorType.AssemblyQualifiedName));

        //    foreach (AnchorStyles style in Enum.GetValues(anchorType))
        //    {
        //        if ((control.Anchor & style) == style)
        //        {
        //            enumEl.Add(new XElement("Item",
        //                new XAttribute("Id", (int)style),
        //                new XAttribute("Name", style.ToString())));
        //        }
        //    }

        //    element.Add(enumEl);
        //}

        // 子控件递归
        if (control.HasChildren)
        {
            foreach (Control child in control.Controls)
            {
                element.Add(SerializeControl(child));
            }
        }

        return element;
    }

    public static void SaveToFile(Form form, string filePath)
    {
        var xml = Serialize(form);
        xml.Save(filePath);
        HtmlLayoutGenerator.SaveToFile(filePath, form.Name + "_layout.html");
    }

    public static Form Deserialize(string filePath)
    {
        var doc = XDocument.Load(filePath);
        return DeserializeForm(doc.Root);
    }

    private static Form DeserializeForm(XElement formElement)
    {
        var form = new Form();
        form.Name = (string)formElement.Attribute("Name");
        form.Text = (string)formElement.Attribute("Text");

        var sizeStr = (string)formElement.Attribute("Size");
        if (SizeConverter.TryParse(sizeStr, out var size))
            form.Size = size;

        var startPosStr = (string)formElement.Attribute("StartPosition");
        if (Enum.TryParse(typeof(FormStartPosition), startPosStr, out object startPos))
            form.StartPosition = (FormStartPosition)startPos;

        foreach (var el in formElement.Elements("Control"))
        {
            var control = DeserializeControl(el);
            form.Controls.Add(control);
        }

        return form;
    }

    private static Control DeserializeControl(XElement el)
    {
        string controlTypeName = (string)el.Attribute("Type");
        string name = (string)el.Attribute("Name");
        string text = (string)el.Attribute("Text");

        var controlType = Type.GetType(controlTypeName);
        var control = (Control)Activator.CreateInstance(controlType);

        control.Name = name;
        control.Text = text;

        var locStr = (string)el.Attribute("Location");
        var sizeStr = (string)el.Attribute("Size");

        if (PointConverter.TryParse(locStr, out var location))
            control.Location = location;

        if (SizeConverter.TryParse(sizeStr, out var size))
            control.Size = size;

        control.Anchor = ParseEnum<AnchorStyles>((string)el.Attribute("Anchor"));
        control.Dock = ParseEnum<DockStyle>((string)el.Attribute("Dock"));
        control.Enabled = (bool)el.Attribute("Enabled");

        // 颜色 & 字体
        control.ForeColor = ColorFromString((string)el.Attribute("ForeColor"));
        control.BackColor = ColorFromString((string)el.Attribute("BackColor"));
        control.Font = FontFromString((string)el.Attribute("Font"));

        // 枚举处理（通用）
        foreach (var enumEl in el.Elements("Enum"))
        {
            string enumTypeName = (string)enumEl.Attribute("Type");
            var enumType = Type.GetType(enumTypeName);

            object enumValue = 0;
            foreach (var item in enumEl.Elements("Item"))
            {
                int id = (int)item.Attribute("Id");
                enumValue = Convert.ChangeType(id, enumType);
            }

            var propInfo = controlType.GetProperty(enumType.Name, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null && propInfo.CanWrite && Enum.IsDefined(enumType, enumValue))
            {
                propInfo.SetValue(control, enumValue);
            }
        }

        // 子控件递归
        foreach (var childEl in el.Elements("Control"))
        {
            var child = DeserializeControl(childEl);
            control.Controls.Add(child);
        }

        return control;
    }

    private static T ParseEnum<T>(string value)
    {
        if (Enum.TryParse(typeof(T), value, out object result))
            return (T)result;
        return default(T);
    }

    private static IEnumerable<Control> GetAllControls(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            yield return control;
            foreach (var child in GetAllControls(control))
                yield return child;
        }
    }

    #region Helper Classes for Parsing

    private static class PointConverter
    {
        public static bool TryParse(string value, out Point point)
        {
            try
            {
                var parts = value.Split(',');
                point = new Point(int.Parse(parts[0]), int.Parse(parts[1]));
                return true;
            }
            catch
            {
                point = Point.Empty;
                return false;
            }
        }
    }

    private static class SizeConverter
    {
        public static bool TryParse(string value, out Size size)
        {
            try
            {
                var parts = value.Split(',');
                size = new Size(int.Parse(parts[0]), int.Parse(parts[1]));
                return true;
            }
            catch
            {
                size = Size.Empty;
                return false;
            }
        }
    }

    #endregion

    #region Utility Methods for Fonts and Colors

    private static string ColorToString(Color color)
    {
        if (color.IsNamedColor)
            return color.Name;
        else
            return $"{color.A},{color.R},{color.G},{color.B}";
    }

    private static Color ColorFromString(string value)
    {
        if (value.StartsWith("#"))
            return ColorTranslator.FromHtml(value);

        if (value.Contains(","))
        {
            var parts = value.Split(',');
            byte a = byte.Parse(parts[0]);
            byte r = byte.Parse(parts[1]);
            byte g = byte.Parse(parts[2]);
            byte b = byte.Parse(parts[3]);
            return Color.FromArgb(a, r, g, b);
        }

        return Color.FromName(value);
    }

    private static string FontToString(Font font)
    {
        return $"{font.FontFamily.Name},{font.Size},{font.Style}";
    }

    private static Font FontFromString(string value)
    {
        try
        {
            var parts = value.Split(',');
            string family = parts[0];
            float size = float.Parse(parts[1]);
            FontStyle style = (FontStyle)Enum.Parse(typeof(FontStyle), parts[2]);
            return new Font(family, size, style);
        }
        catch
        {
            return SystemFonts.DefaultFont;
        }
    }

    #endregion
}