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
            Pan_Background = new AntdUI.Panel();
            Lab_Console = new AntdUI.Label();
            SuspendLayout();
            // 
            // Pan_Background
            // 
            Pan_Background.Dock = DockStyle.Fill;
            Pan_Background.Location = new Point(0, 0);
            Pan_Background.Name = "Pan_Background";
            Pan_Background.Size = new Size(1258, 664);
            Pan_Background.TabIndex = 0;
            Pan_Background.Text = "panel1";
            // 
            // Lab_Console
            // 
            Lab_Console.Dock = DockStyle.Bottom;
            Lab_Console.Location = new Point(0, 630);
            Lab_Console.Name = "Lab_Console";
            Lab_Console.Size = new Size(1258, 34);
            Lab_Console.TabIndex = 1;
            Lab_Console.Text = "控制台";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1258, 664);
            Controls.Add(Lab_Console);
            Controls.Add(Pan_Background);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
        }

        #endregion

        private AntdUI.Panel Pan_Background;
        private AntdUI.Label Lab_Console;
    }
}
