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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            this.lbProcessExceptions = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
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
            this.lbProcessExceptions.Size = new System.Drawing.Size(149, 232);
            this.lbProcessExceptions.TabIndex = 0;
            this.lbProcessExceptions.DoubleClick += new System.EventHandler(this.LbProcessExceptions_DoubleClick);
            this.lbProcessExceptions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LbProcessExceptions_KeyDown);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbProcessExceptions);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(163, 266);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Process Exceptions";
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
            this.groupBox2.Size = new System.Drawing.Size(153, 266);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rendering";
            // 
            // chbSpawnOnClick
            // 
            this.chbSpawnOnClick.AutoSize = true;
            this.chbSpawnOnClick.Checked = true;
            this.chbSpawnOnClick.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbSpawnOnClick.Location = new System.Drawing.Point(6, 78);
            this.chbSpawnOnClick.Name = "chbSpawnOnClick";
            this.chbSpawnOnClick.Size = new System.Drawing.Size(133, 23);
            this.chbSpawnOnClick.TabIndex = 2;
            this.chbSpawnOnClick.Text = "Spawn On Click";
            this.chbSpawnOnClick.UseVisualStyleBackColor = true;
            this.chbSpawnOnClick.CheckedChanged += new System.EventHandler(this.Chb_CheckedChanged);
            // 
            // chbDrawSineWave
            // 
            this.chbDrawSineWave.AutoSize = true;
            this.chbDrawSineWave.Checked = true;
            this.chbDrawSineWave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbDrawSineWave.Location = new System.Drawing.Point(6, 136);
            this.chbDrawSineWave.Name = "chbDrawSineWave";
            this.chbDrawSineWave.Size = new System.Drawing.Size(139, 23);
            this.chbDrawSineWave.TabIndex = 2;
            this.chbDrawSineWave.Text = "Draw Sine Wave";
            this.chbDrawSineWave.UseVisualStyleBackColor = true;
            this.chbDrawSineWave.CheckedChanged += new System.EventHandler(this.Chb_CheckedChanged);
            // 
            // chbBurningTaskBar
            // 
            this.chbBurningTaskBar.AutoSize = true;
            this.chbBurningTaskBar.Checked = true;
            this.chbBurningTaskBar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbBurningTaskBar.Location = new System.Drawing.Point(6, 107);
            this.chbBurningTaskBar.Name = "chbBurningTaskBar";
            this.chbBurningTaskBar.Size = new System.Drawing.Size(142, 23);
            this.chbBurningTaskBar.TabIndex = 2;
            this.chbBurningTaskBar.Text = "Burning Task Bar";
            this.chbBurningTaskBar.UseVisualStyleBackColor = true;
            this.chbBurningTaskBar.CheckedChanged += new System.EventHandler(this.Chb_CheckedChanged);
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
            this.nudFramerate.ValueChanged += new System.EventHandler(this.NudFramerate_ValueChanged);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 290);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(362, 329);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(362, 329);
            this.Name = "ConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Wallpaper Animator Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbProcessExceptions;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudFramerate;
        private System.Windows.Forms.CheckBox chbSpawnOnClick;
        private System.Windows.Forms.CheckBox chbDrawSineWave;
        private System.Windows.Forms.CheckBox chbBurningTaskBar;
    }
}