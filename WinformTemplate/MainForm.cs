using WinformTemplate.Logger;
using WinformTemplate.UIComponent;

namespace WinformTemplate
{
    /// <summary>
    /// @View
    ///     主程序UI入口
    /// </summary>
    public partial class MainForm : Form
    {
        // UI 控制台
        private readonly LabelWriter? _labelWriter;

        public MainForm()
        {
            InitializeComponent();

            // 创建并设置默认 UI Console
            _labelWriter ??= new LabelWriter(this.Lab_Console);
            Console.SetOut(_labelWriter);

            // 加载主窗体信息时显示版本 
            LoadBaseInfo();
        }

        /// <summary>
        /// 加载窗体时，上方显示信息+版本号
        /// </summary>
        private void LoadBaseInfo()
        {
            var title = this.Text + " --- " + Application.ProductVersion;
            this.Text = title;
        }

        /// <summary>
        /// 切换选项卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab_Main_SelectedIndexChanged(object sender, AntdUI.IntEventArgs e)
        {
            var selectPage = this.Tab_Main.SelectedTab;
            var selectIndex = this.Tab_Main.SelectedIndex;
            selectPage?.Controls.Clear();

            UserControl? userControl = null;

            switch (selectIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
            }

            if (userControl == null) return;

            userControl.Dock = DockStyle.Fill;
            selectPage?.Controls.Add(userControl);

            Debug.Info($"{this.GetType().Name} 切换页面 {selectIndex}");
        }

        /// <summary>
        /// 拉动窗体让控制台侧保持一个自适应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // 检查窗体是否最小化
            if (this.WindowState == FormWindowState.Minimized)
            {
                // 如果最小化，不执行 SplitterDistance 的设置
                return;
            }

            var dis = Math.Abs(this.Size.Height - Lab_Console.Size.Height);

            // 在窗体正常或最大化时设置 SplitterDistance
            var newSplitterDistance = Math.Max(
                SContainer_Main.Panel1MinSize, Math.Min(SContainer_Main.Width - SContainer_Main.Panel2MinSize, dis)
            );

            SContainer_Main.SplitterDistance = newSplitterDistance;
        }

        #region Function

        /// <summary>
        /// 初次加载页面
        /// </summary>
        /// <param name="firstLoad"></param>
        private void FirstLoadTabPage(UserControl firstLoad)
        {
            var selectPage = this.Tab_Main.SelectedTab;
            firstLoad.Dock = DockStyle.Fill;
            selectPage?.Controls.Add(firstLoad);
        }

        #endregion
    }
}
