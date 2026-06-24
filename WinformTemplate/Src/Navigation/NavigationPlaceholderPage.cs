namespace WinformTemplate.Navigation;

public sealed class NavigationPlaceholderPage : UserControl
{
    private NavigationPlaceholderPage(string title, string detail)
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        var titleLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 44,
            Text = title,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
            Padding = new Padding(24, 12, 24, 0)
        };

        var detailLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 36,
            Text = detail,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(24, 4, 24, 0)
        };

        Controls.Add(detailLabel);
        Controls.Add(titleLabel);
    }

    public static NavigationPlaceholderPage AccessDenied(string menuKey)
    {
        return new NavigationPlaceholderPage("无权限", $"当前账户无权访问 {menuKey}");
    }

    public static NavigationPlaceholderPage NotImplemented(string menuKey)
    {
        return new NavigationPlaceholderPage("功能未实现", $"{menuKey} 尚未注册页面");
    }
}
