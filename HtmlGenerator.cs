using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;

public static class HtmlLayoutGenerator
{
    public static string Generate(string xmlFilePath)
    {
        var doc = XDocument.Load(xmlFilePath);
        var formElement = doc.Element("Form");

        StringBuilder html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <title>Form Layout Preview</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { position: relative; margin: 0; padding: 0; }");
        html.AppendLine("        .control { box-sizing: border-box; padding: 4px; overflow: hidden; }");
        html.AppendLine("        .button { border: 1px solid #ccc; text-align: center; line-height: 28px; cursor: default; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        foreach (var controlEl in formElement.Elements("Control"))
        {
            html.Append(RenderControl(controlEl));
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static string RenderControl(XElement el)
    {
        var sb = new StringBuilder();
        string type = (string)el.Attribute("Type");
        string name = (string)el.Attribute("Name");
        string text = (string)el.Attribute("Text");
        string locationStr = (string)el.Attribute("Location");
        string sizeStr = (string)el.Attribute("Size");
        bool enabled = (bool)el.Attribute("Enabled"); // 默认值为true
        string foreColor = (string)el.Attribute("ForeColor");
        string backColor = (string)el.Attribute("BackColor");
        string fontStr = (string)el.Attribute("Font");

        // 解析位置和尺寸
        Point location = ParsePoint(locationStr);
        Size size = ParseSize(sizeStr);

        // 构建样式字符串
        StringBuilder style = new StringBuilder();
        style.AppendFormat("position: absolute; ");
        style.AppendFormat("left: {0}px; top: {1}px; ", location.X, location.Y);
        style.AppendFormat("width: {0}px; height: {1}px; ", size.Width, size.Height);

        if (!string.IsNullOrEmpty(backColor))
            style.AppendFormat("background-color: {0}; ", ColorToHtml(backColor));

        if (!string.IsNullOrEmpty(foreColor))
            style.AppendFormat("color: {0}; ", ColorToHtml(foreColor));

        if (!string.IsNullOrEmpty(fontStr))
        {
            var fontParts = fontStr.Split(',');
            if (fontParts.Length >= 3)
            {
                style.AppendFormat("font-family: '{0}'; font-size: {1}px; ", fontParts[0], fontParts[1]);
                if (fontParts.Length > 2 && !string.IsNullOrEmpty(fontParts[2]))
                {
                    style.AppendFormat("font-weight: {0}; ", fontParts[2].ToLower().Contains("bold") ? "bold" : "normal");
                    style.AppendFormat("font-style: {0}; ", fontParts[2].ToLower().Contains("italic") ? "italic" : "normal");
                }
            }
        }

        // 构建 HTML 元素
        string tagName = "div";
        string cssClass = "control";

        if (type.Contains("Button"))
        {
            cssClass += " button";
            tagName = "button";
        }
        else if (type.Contains("TextBox"))
        {
            tagName = "input";
            cssClass += " textbox";
        }
        else if (type.Contains("Label"))
        {
            tagName = "label";
            cssClass += " label";
        }
        else if (type.Contains("Panel"))
        {
            cssClass += " panel";
        }

        sb.AppendFormat("<{0} class=\"{1}\" style=\"{2}\"", tagName, cssClass, style.ToString().TrimEnd(' ', ';'));

        if (tagName == "input")
        {
            sb.AppendFormat(" type=\"text\" value=\"{0}\" />", text);
        }
        else if (tagName == "button")
        {
            sb.Append(">");
            sb.Append(text);
            sb.AppendFormat("</{0}>", tagName);
        }
        else if (tagName != "div")
        {
            sb.Append(">");
            sb.Append(text);
            sb.AppendFormat("</{0}>", tagName);
        }
        else
        {
            sb.Append(">");
            sb.Append(text);
            sb.AppendFormat("</{0}>", tagName);
        }

        sb.AppendLine();

        // 子控件递归
        foreach (var childEl in el.Elements("Control"))
        {
            sb.Append(RenderControl(childEl));
        }

        return sb.ToString();
    }

    private static Point ParsePoint(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new Point(0, 0);

        var parts = value.Split(',');
        return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static Size ParseSize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new Size(100, 20);

        var parts = value.Split(',');
        return new Size(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static string ColorToHtml(string value)
    {
        if (value.StartsWith("#"))
            return value;

        if (value.Contains(","))
        {
            var parts = value.Split(',');
            if (parts.Length == 4)
            {
                byte a = byte.Parse(parts[0]);
                byte r = byte.Parse(parts[1]);
                byte g = byte.Parse(parts[2]);
                byte b = byte.Parse(parts[3]);

                return $"rgba({r}, {g}, {b}, {a / 255.0})";
            }
        }

        try
        {
            return "#" + Color.FromName(value).ToArgb().ToString("X6");
        }
        catch
        {
            return "#000000";
        }
    }

    public static void SaveToFile(string xmlPath, string htmlPath)
    {
        string html = Generate(xmlPath);
        File.WriteAllText(htmlPath, html);
    }
}