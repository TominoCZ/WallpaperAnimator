using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using WallpaperAnimator.Properties;

namespace WallpaperAnimator
{
    public partial class ConfigForm : Form
    {
        private string processName;

        public ConfigForm()
        {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            Settings.Default.Reload();

            var processes = Settings.Default.ProcessExceptions;

            if (processes == null)
                return;

            for (var index = 0; index < processes.Count; index++)
            {
                var process = processes[index];
                lbProcessExceptions.Items.Add(process);
            }

            nudFramerate.Value = Settings.Default.FramerateLimit;

            chbSpawnOnClick.Checked = Settings.Default.SpawnOnClick;
            chbDrawSineWave.Checked = Settings.Default.DrawSineWave;
            chbBurningTaskBar.Checked = Settings.Default.BurningTaskBar;
        }

        private void SaveSettings()
        {
            var processes = new StringCollection();
            for (var index = 0; index < lbProcessExceptions.Items.Count; index++)
            {
                var item = lbProcessExceptions.Items[index];

                processes.Add((string)item);
            }

            Settings.Default.FramerateLimit = (int)nudFramerate.Value;
            Settings.Default.ProcessExceptions = processes;

            Settings.Default.SpawnOnClick = chbSpawnOnClick.Checked;
            Settings.Default.DrawSineWave = chbDrawSineWave.Checked;
            Settings.Default.BurningTaskBar = chbBurningTaskBar.Checked;

            Settings.Default.Save();
        }

        private void chb_CheckedChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void lbProcessExceptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lbProcessExceptions.SelectedItem is string s)
            {
                var result = MessageBox.Show($"Delte '{s}'?", "Delete", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    lbProcessExceptions.Items.Remove(lbProcessExceptions.SelectedItem);
                    SaveSettings();
                }
            }
        }

        private void lbProcessExceptions_DoubleClick(object sender, EventArgs e)
        {
            var form = new AddProcessForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                lbProcessExceptions.Items.Add(form.ProcessName);
                SaveSettings();
            }
        }

        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }
    }
}
