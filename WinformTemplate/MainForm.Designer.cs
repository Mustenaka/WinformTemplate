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
            Lab_Console = new AntdUI.Label();
            SContainer_Main = new SplitContainer();
            ((System.ComponentModel.ISupportInitialize)SContainer_Main).BeginInit();
            SContainer_Main.Panel2.SuspendLayout();
            SContainer_Main.SuspendLayout();
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
            // SContainer_Main.Panel2
            // 
            SContainer_Main.Panel2.Controls.Add(Lab_Console);
            SContainer_Main.Size = new Size(1658, 968);
            SContainer_Main.SplitterDistance = 939;
            SContainer_Main.TabIndex = 2;
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
            SContainer_Main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SContainer_Main).EndInit();
            SContainer_Main.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private AntdUI.Label Lab_Console;
        private SplitContainer SContainer_Main;
    }
}
