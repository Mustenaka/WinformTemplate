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
            tabs1 = new AntdUI.Tabs();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            menu1 = new AntdUI.Menu();
            menu2 = new AntdUI.Menu();
            button1 = new AntdUI.Button();
            label1 = new AntdUI.Label();
            select1 = new AntdUI.Select();
            slider1 = new AntdUI.Slider();
            switch1 = new AntdUI.Switch();
            timePicker1 = new AntdUI.TimePicker();
            tooltip1 = new AntdUI.Tooltip();
            tooltip2 = new AntdUI.Tooltip();
            timeline1 = new AntdUI.Timeline();
            textBox1 = new TextBox();
            tabs1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // tabs1
            // 
            tabs1.Controls.Add(tabPage1);
            tabs1.Controls.Add(tabPage2);
            tabs1.Location = new Point(-1, 2);
            tabs1.Name = "tabs1";
            tabs1.SelectedIndex = 0;
            tabs1.Size = new Size(1264, 624);
            tabs1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 30);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1256, 629);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "SKY";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(timeline1);
            tabPage2.Controls.Add(tooltip2);
            tabPage2.Controls.Add(tooltip1);
            tabPage2.Controls.Add(timePicker1);
            tabPage2.Controls.Add(switch1);
            tabPage2.Controls.Add(slider1);
            tabPage2.Controls.Add(select1);
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(button1);
            tabPage2.Controls.Add(menu2);
            tabPage2.Controls.Add(menu1);
            tabPage2.Location = new Point(4, 30);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1256, 590);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "华宁";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // menu1
            // 
            menu1.BackColor = Color.DimGray;
            menu1.Location = new Point(3, 0);
            menu1.Name = "menu1";
            menu1.Size = new Size(147, 626);
            menu1.TabIndex = 0;
            menu1.Text = "menu1";
            // 
            // menu2
            // 
            menu2.BackColor = Color.DarkGray;
            menu2.Location = new Point(147, 0);
            menu2.Name = "menu2";
            menu2.Size = new Size(147, 626);
            menu2.TabIndex = 1;
            menu2.Text = "menu2";
            // 
            // button1
            // 
            button1.Location = new Point(329, 61);
            button1.Name = "button1";
            button1.Size = new Size(161, 55);
            button1.TabIndex = 2;
            button1.Text = "button1";
            // 
            // label1
            // 
            label1.Location = new Point(329, 21);
            label1.Name = "label1";
            label1.Size = new Size(112, 34);
            label1.TabIndex = 3;
            label1.Text = "label1";
            // 
            // select1
            // 
            select1.Location = new Point(320, 137);
            select1.Name = "select1";
            select1.Size = new Size(251, 54);
            select1.TabIndex = 4;
            select1.Text = "select1";
            // 
            // slider1
            // 
            slider1.Location = new Point(320, 211);
            slider1.Name = "slider1";
            slider1.Size = new Size(362, 55);
            slider1.TabIndex = 5;
            slider1.Text = "slider1";
            // 
            // switch1
            // 
            switch1.Location = new Point(329, 290);
            switch1.Name = "switch1";
            switch1.Size = new Size(112, 34);
            switch1.TabIndex = 6;
            switch1.Text = "switch1";
            // 
            // timePicker1
            // 
            timePicker1.Location = new Point(329, 365);
            timePicker1.Name = "timePicker1";
            timePicker1.Size = new Size(112, 34);
            timePicker1.TabIndex = 7;
            timePicker1.Text = "timePicker1";
            // 
            // tooltip1
            // 
            tooltip1.Location = new Point(337, 373);
            tooltip1.MaximumSize = new Size(96, 54);
            tooltip1.MinimumSize = new Size(96, 54);
            tooltip1.Name = "tooltip1";
            tooltip1.Size = new Size(96, 54);
            tooltip1.TabIndex = 8;
            tooltip1.Text = "tooltip1";
            // 
            // tooltip2
            // 
            tooltip2.Location = new Point(548, 21);
            tooltip2.MaximumSize = new Size(96, 54);
            tooltip2.MinimumSize = new Size(96, 54);
            tooltip2.Name = "tooltip2";
            tooltip2.Size = new Size(96, 54);
            tooltip2.TabIndex = 9;
            tooltip2.Text = "tooltip2";
            // 
            // timeline1
            // 
            timeline1.Location = new Point(617, 583);
            timeline1.Name = "timeline1";
            timeline1.Size = new Size(112, 34);
            timeline1.TabIndex = 10;
            timeline1.Text = "timeline1";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(3, 628);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(1256, 30);
            textBox1.TabIndex = 11;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1258, 664);
            Controls.Add(textBox1);
            Controls.Add(tabs1);
            Name = "MainForm";
            Text = "MainForm";
            tabs1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private AntdUI.Tabs tabs1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private AntdUI.Tooltip tooltip2;
        private AntdUI.Tooltip tooltip1;
        private AntdUI.TimePicker timePicker1;
        private AntdUI.Switch switch1;
        private AntdUI.Slider slider1;
        private AntdUI.Select select1;
        private AntdUI.Label label1;
        private AntdUI.Button button1;
        private AntdUI.Menu menu2;
        private AntdUI.Menu menu1;
        private AntdUI.Timeline timeline1;
        private TextBox textBox1;
    }
}
