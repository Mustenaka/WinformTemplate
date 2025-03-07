using WinformTemplate.Logger;
using WinformTemplate.UIComponent;

namespace WinformTemplate
{
    /// <summary>
    /// @View
    ///     ������UI���
    /// </summary>
    public partial class MainForm : Form
    {
        // UI ����̨
        private readonly LabelWriter? _labelWriter;

        public MainForm()
        {
            InitializeComponent();

            // ����������Ĭ�� UI Console
            _labelWriter ??= new LabelWriter(this.Lab_Console);
            Console.SetOut(_labelWriter);

            // ������������Ϣʱ��ʾ�汾 
            LoadBaseInfo();
        }

        /// <summary>
        /// ���ش���ʱ���Ϸ���ʾ��Ϣ+�汾��
        /// </summary>
        private void LoadBaseInfo()
        {
            var title = this.Text + " --- " + Application.ProductVersion;
            this.Text = title;
        }

        /// <summary>
        /// �л�ѡ�
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

            Debug.Info($"{this.GetType().Name} �л�ҳ�� {selectIndex}");
        }

        /// <summary>
        /// ���������ÿ���̨�ౣ��һ������Ӧ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // ��鴰���Ƿ���С��
            if (this.WindowState == FormWindowState.Minimized)
            {
                // �����С������ִ�� SplitterDistance ������
                return;
            }

            var dis = Math.Abs(this.Size.Height - Lab_Console.Size.Height);

            // �ڴ������������ʱ���� SplitterDistance
            var newSplitterDistance = Math.Max(
                SContainer_Main.Panel1MinSize, Math.Min(SContainer_Main.Width - SContainer_Main.Panel2MinSize, dis)
            );

            SContainer_Main.SplitterDistance = newSplitterDistance;
        }

        #region Function

        /// <summary>
        /// ���μ���ҳ��
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
