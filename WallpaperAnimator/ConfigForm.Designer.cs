using System;

namespace WallpaperAnimator
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbProcessExceptions = new System.Windows.Forms.ListBox();
            this.tbProcessName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAddException = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chbSpawnOnClick = new System.Windows.Forms.CheckBox();
            this.chbDrawSineWave = new System.Windows.Forms.CheckBox();
            this.chbBurningTaskBar = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nudFramerate = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).BeginInit();
            this.SuspendLayout();
            // 
            // lbProcessExceptions
            // 
            this.lbProcessExceptions.FormattingEnabled = true;
            this.lbProcessExceptions.IntegralHeight = false;
            this.lbProcessExceptions.ItemHeight = 19;
            this.lbProcessExceptions.Location = new System.Drawing.Point(7, 27);
            this.lbProcessExceptions.Margin = new System.Windows.Forms.Padding(4);
            this.lbProcessExceptions.Name = "lbProcessExceptions";
            this.lbProcessExceptions.Size = new System.Drawing.Size(149, 158);
            this.lbProcessExceptions.TabIndex = 0;
            this.lbProcessExceptions.DoubleClick += new System.EventHandler(this.lbProcessExceptions_DoubleClick);
            this.lbProcessExceptions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbProcessExceptions_KeyDown);
            // 
            // tbProcessName
            // 
            this.tbProcessName.Location = new System.Drawing.Point(6, 192);
            this.tbProcessName.Name = "tbProcessName";
            this.tbProcessName.Size = new System.Drawing.Size(151, 27);
            this.tbProcessName.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAddException);
            this.groupBox1.Controls.Add(this.tbProcessName);
            this.groupBox1.Controls.Add(this.lbProcessExceptions);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(163, 271);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Process Exceptions";
            // 
            // btnAddException
            // 
            this.btnAddException.Enabled = false;
            this.btnAddException.Location = new System.Drawing.Point(6, 225);
            this.btnAddException.Name = "btnAddException";
            this.btnAddException.Size = new System.Drawing.Size(151, 40);
            this.btnAddException.TabIndex = 3;
            this.btnAddException.Text = "ADD";
            this.btnAddException.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chbSpawnOnClick);
            this.groupBox2.Controls.Add(this.chbDrawSineWave);
            this.groupBox2.Controls.Add(this.chbBurningTaskBar);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.nudFramerate);
            this.groupBox2.Location = new System.Drawing.Point(181, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(156, 271);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rendering";
            // 
            // chbSpawnOnClick
            // 
            this.chbSpawnOnClick.AutoSize = true;
            this.chbSpawnOnClick.Location = new System.Drawing.Point(6, 78);
            this.chbSpawnOnClick.Name = "chbSpawnOnClick";
            this.chbSpawnOnClick.Size = new System.Drawing.Size(133, 23);
            this.chbSpawnOnClick.TabIndex = 2;
            this.chbSpawnOnClick.Text = "Spawn On Click";
            this.chbSpawnOnClick.UseVisualStyleBackColor = true;
            this.chbSpawnOnClick.CheckedChanged += new System.EventHandler(this.chb_CheckedChanged);
            // 
            // chbDrawSineWave
            // 
            this.chbDrawSineWave.AutoSize = true;
            this.chbDrawSineWave.Location = new System.Drawing.Point(6, 136);
            this.chbDrawSineWave.Name = "chbDrawSineWave";
            this.chbDrawSineWave.Size = new System.Drawing.Size(139, 23);
            this.chbDrawSineWave.TabIndex = 2;
            this.chbDrawSineWave.Text = "Draw Sine Wave";
            this.chbDrawSineWave.UseVisualStyleBackColor = true;
            this.chbDrawSineWave.CheckedChanged += new System.EventHandler(this.chb_CheckedChanged);
            // 
            // chbBurningTaskBar
            // 
            this.chbBurningTaskBar.AutoSize = true;
            this.chbBurningTaskBar.Location = new System.Drawing.Point(6, 107);
            this.chbBurningTaskBar.Name = "chbBurningTaskBar";
            this.chbBurningTaskBar.Size = new System.Drawing.Size(142, 23);
            this.chbBurningTaskBar.TabIndex = 2;
            this.chbBurningTaskBar.Text = "Burning Task Bar";
            this.chbBurningTaskBar.UseVisualStyleBackColor = true;
            this.chbBurningTaskBar.CheckedChanged += new System.EventHandler(this.chb_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "Framerate Limit";
            // 
            // nudFramerate
            // 
            this.nudFramerate.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudFramerate.Location = new System.Drawing.Point(6, 45);
            this.nudFramerate.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.nudFramerate.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudFramerate.Name = "nudFramerate";
            this.nudFramerate.Size = new System.Drawing.Size(115, 27);
            this.nudFramerate.TabIndex = 0;
            this.nudFramerate.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 294);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "ConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Wallpaper Animator Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbProcessExceptions;
        private System.Windows.Forms.TextBox tbProcessName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAddException;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudFramerate;
        private System.Windows.Forms.CheckBox chbSpawnOnClick;
        private System.Windows.Forms.CheckBox chbDrawSineWave;
        private System.Windows.Forms.CheckBox chbBurningTaskBar;
    }
}