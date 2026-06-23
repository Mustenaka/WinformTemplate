namespace WinformTemplate.UI.Business.Sys.Role;

public sealed class RolePlaceholderControl : UserControl
{
    public RolePlaceholderControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        var title = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 44,
            Text = "角色管理",
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
            Padding = new Padding(24, 12, 24, 0)
        };

        var detail = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 36,
            Text = "角色管理完整业务将在 P4 实现。",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(24, 4, 24, 0)
        };

        Controls.Add(detail);
        Controls.Add(title);
    }
}
