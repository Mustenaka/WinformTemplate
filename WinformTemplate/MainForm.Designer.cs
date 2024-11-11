namespace WinformTemplate
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            AntdUI.Tabs.StyleLine styleLine1 = new AntdUI.Tabs.StyleLine();
            Lab_Console = new AntdUI.Label();
            SContainer_Main = new SplitContainer();
            Panel_Bg = new AntdUI.Panel();
            Tab_Main = new AntdUI.Tabs();
            TPageTest01 = new AntdUI.TabPage();
            TPageTest02 = new AntdUI.TabPage();
            ((System.ComponentModel.ISupportInitialize)SContainer_Main).BeginInit();
            SContainer_Main.Panel1.SuspendLayout();
            SContainer_Main.Panel2.SuspendLayout();
            SContainer_Main.SuspendLayout();
            Panel_Bg.SuspendLayout();
            Tab_Main.SuspendLayout();
            SuspendLayout();
            // 
            // Lab_Console
            // 
            Lab_Console.Dock = DockStyle.Fill;
            Lab_Console.Location = new Point(0, 0);
            Lab_Console.Name = "Lab_Console";
            Lab_Console.Size = new Size(1658, 25);
            Lab_Console.TabIndex = 1;
            Lab_Console.Text = "控制台";
            // 
            // SContainer_Main
            // 
            SContainer_Main.Dock = DockStyle.Fill;
            SContainer_Main.IsSplitterFixed = true;
            SContainer_Main.Location = new Point(0, 0);
            SContainer_Main.Name = "SContainer_Main";
            SContainer_Main.Orientation = Orientation.Horizontal;
            // 
            // SContainer_Main.Panel1
            // 
            SContainer_Main.Panel1.Controls.Add(Panel_Bg);
            // 
            // SContainer_Main.Panel2
            // 
            SContainer_Main.Panel2.Controls.Add(Lab_Console);
            SContainer_Main.Size = new Size(1658, 968);
            SContainer_Main.SplitterDistance = 939;
            SContainer_Main.TabIndex = 2;
            // 
            // Panel_Bg
            // 
            Panel_Bg.Controls.Add(Tab_Main);
            Panel_Bg.Dock = DockStyle.Fill;
            Panel_Bg.Location = new Point(0, 0);
            Panel_Bg.Name = "Panel_Bg";
            Panel_Bg.Size = new Size(1658, 939);
            Panel_Bg.TabIndex = 0;
            Panel_Bg.Text = "panel1";
            // 
            // Tab_Main
            // 
            Tab_Main.Dock = DockStyle.Fill;
            Tab_Main.Location = new Point(0, 0);
            Tab_Main.Name = "Tab_Main";
            Tab_Main.Pages.Add(TPageTest01);
            Tab_Main.Pages.Add(TPageTest02);
            Tab_Main.Size = new Size(1658, 939);
            Tab_Main.Style = styleLine1;
            Tab_Main.TabIndex = 0;
            Tab_Main.Text = "tabs1";
            Tab_Main.SelectedIndexChanged += Tab_Main_SelectedIndexChanged;
            // 
            // TPageTest01
            // 
            TPageTest01.Dock = DockStyle.Fill;
            TPageTest01.Location = new Point(3, 41);
            TPageTest01.Name = "TPageTest01";
            TPageTest01.Size = new Size(1652, 895);
            TPageTest01.TabIndex = 0;
            TPageTest01.Text = "测试页1";
            // 
            // TPageTest02
            // 
            TPageTest02.Dock = DockStyle.Fill;
            TPageTest02.Location = new Point(0, 0);
            TPageTest02.Name = "TPageTest02";
            TPageTest02.Size = new Size(0, 0);
            TPageTest02.TabIndex = 0;
            TPageTest02.Text = "测试页2";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1658, 968);
            Controls.Add(SContainer_Main);
            MinimumSize = new Size(320, 160);
            Name = "MainForm";
            Text = "MainForm";
            SizeChanged += MainForm_SizeChanged;
            SContainer_Main.Panel1.ResumeLayout(false);
            SContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SContainer_Main).EndInit();
            SContainer_Main.ResumeLayout(false);
            Panel_Bg.ResumeLayout(false);
            Tab_Main.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.Label Lab_Console;
        private SplitContainer SContainer_Main;
        private AntdUI.Panel Panel_Bg;
        private AntdUI.Tabs Tab_Main;
        private AntdUI.TabPage TPageTest01;
        private AntdUI.TabPage TPageTest02;
    }
}
