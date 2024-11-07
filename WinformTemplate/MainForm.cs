namespace WinformTemplate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 拉动窗体让控制台侧保持一个自适应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //var dis = Math.Abs(this.Size.Height - Lab_Console.Size.Height);
            //SContainer_Main.SplitterDistance = dis;

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
    }
}
