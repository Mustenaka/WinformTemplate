using System.Text;

namespace WinformTemplate.UIComponent;

/// <summary>
/// 配合AntdUI的一个终端控制台输出接口
/// </summary>
public class LabelWriter : TextWriter
{
    private readonly AntdUI.Label _textBox;

    public LabelWriter(AntdUI.Label textBox)
    {
        _textBox = textBox;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        base.Write(value);
        UpdateLabel(value.ToString());
    }

    public override void Write(string? value)
    {
        base.Write(value);
        if (value != null) UpdateLabel(value);
    }

    public override void WriteLine(string? value)
    {
        base.WriteLine(value);
        UpdateLabel(value + Environment.NewLine);
    }

    private void UpdateLabel(string value)
    {
        if (_textBox.InvokeRequired)
        {
            _textBox.Invoke(new Action<string>(UpdateLabel), value);
        }
        else
        {
            _textBox.Text = value;
        }
    }
}