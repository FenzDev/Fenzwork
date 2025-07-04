namespace WinFormsApp1
{
    partial class Form1
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
            Name1 = new TextBox();
            Name0 = new TextBox();
            Name2 = new TextBox();
            Name3 = new TextBox();
            Name4 = new TextBox();
            Count0 = new TextBox();
            Count1 = new TextBox();
            Count2 = new TextBox();
            Count3 = new TextBox();
            Count4 = new TextBox();
            TemplateBox = new TextBox();
            OutputBox = new TextBox();
            label6 = new Label();
            label7 = new Label();
            button1 = new Button();
            SuspendLayout();
            // 
            // Name1
            // 
            Name1.AccessibleName = "Name2";
            Name1.Location = new Point(12, 41);
            Name1.Name = "Name1";
            Name1.ReadOnly = true;
            Name1.Size = new Size(110, 23);
            Name1.TabIndex = 0;
            // 
            // Name0
            // 
            Name0.AccessibleName = "Name1";
            Name0.Location = new Point(12, 12);
            Name0.Name = "Name0";
            Name0.ReadOnly = true;
            Name0.Size = new Size(110, 23);
            Name0.TabIndex = 1;
            // 
            // Name2
            // 
            Name2.AccessibleName = "Name3";
            Name2.Location = new Point(12, 70);
            Name2.Name = "Name2";
            Name2.ReadOnly = true;
            Name2.Size = new Size(110, 23);
            Name2.TabIndex = 4;
            // 
            // Name3
            // 
            Name3.AccessibleName = "Name4";
            Name3.Location = new Point(12, 99);
            Name3.Name = "Name3";
            Name3.ReadOnly = true;
            Name3.Size = new Size(110, 23);
            Name3.TabIndex = 5;
            Name3.TextChanged += Name3_TextChanged;
            // 
            // Name4
            // 
            Name4.AccessibleName = "Name5";
            Name4.Location = new Point(12, 128);
            Name4.Name = "Name4";
            Name4.ReadOnly = true;
            Name4.Size = new Size(110, 23);
            Name4.TabIndex = 6;
            // 
            // Count0
            // 
            Count0.AccessibleName = "Count1";
            Count0.Location = new Point(128, 12);
            Count0.Name = "Count0";
            Count0.Size = new Size(43, 23);
            Count0.TabIndex = 10;
            // 
            // Count1
            // 
            Count1.AccessibleName = "Count2";
            Count1.Location = new Point(128, 41);
            Count1.Name = "Count1";
            Count1.Size = new Size(43, 23);
            Count1.TabIndex = 11;
            // 
            // Count2
            // 
            Count2.AccessibleName = "Count3";
            Count2.Location = new Point(128, 70);
            Count2.Name = "Count2";
            Count2.Size = new Size(43, 23);
            Count2.TabIndex = 12;
            // 
            // Count3
            // 
            Count3.AccessibleName = "Count4";
            Count3.Location = new Point(128, 99);
            Count3.Name = "Count3";
            Count3.Size = new Size(43, 23);
            Count3.TabIndex = 13;
            // 
            // Count4
            // 
            Count4.AccessibleName = "Count5";
            Count4.Location = new Point(128, 128);
            Count4.Name = "Count4";
            Count4.Size = new Size(43, 23);
            Count4.TabIndex = 14;
            // 
            // TemplateBox
            // 
            TemplateBox.AccessibleName = "TemplatesBox";
            TemplateBox.Location = new Point(284, 12);
            TemplateBox.Name = "TemplateBox";
            TemplateBox.Size = new Size(92, 23);
            TemplateBox.TabIndex = 15;
            // 
            // OutputBox
            // 
            OutputBox.AccessibleName = "OutputBox";
            OutputBox.Location = new Point(284, 41);
            OutputBox.Name = "OutputBox";
            OutputBox.Size = new Size(92, 23);
            OutputBox.TabIndex = 16;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(213, 15);
            label6.Name = "label6";
            label6.Size = new Size(60, 15);
            label6.TabIndex = 17;
            label6.Text = "Templates";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(213, 44);
            label7.Name = "label7";
            label7.Size = new Size(45, 15);
            label7.TabIndex = 18;
            label7.Text = "Output";
            // 
            // button1
            // 
            button1.Location = new Point(261, 121);
            button1.Name = "button1";
            button1.Size = new Size(115, 30);
            button1.TabIndex = 19;
            button1.Text = "Generate Clones";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(392, 163);
            Controls.Add(button1);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(OutputBox);
            Controls.Add(TemplateBox);
            Controls.Add(Count4);
            Controls.Add(Count3);
            Controls.Add(Count2);
            Controls.Add(Count1);
            Controls.Add(Count0);
            Controls.Add(Name4);
            Controls.Add(Name3);
            Controls.Add(Name2);
            Controls.Add(Name0);
            Controls.Add(Name1);
            Name = "Form1";
            Text = "Sprite Files Cloner";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox Name1;
        private TextBox Name0;
        private TextBox Name2;
        private TextBox Name3;
        private TextBox Name4;
        private TextBox Count0;
        private TextBox Count1;
        private TextBox Count2;
        private TextBox Count3;
        private TextBox Count4;
        private TextBox TemplateBox;
        private TextBox OutputBox;
        private Label label6;
        private Label label7;
        private Button button1;
    }
}
