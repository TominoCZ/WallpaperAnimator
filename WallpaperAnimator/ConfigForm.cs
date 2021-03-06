﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using WallpaperAnimator.Properties;

namespace WallpaperAnimator
{
    public partial class ConfigForm : Form
    {
        private bool _loaded;

        public ConfigForm()
        {
            InitializeComponent();

            LoadSettings();
        }

        private void LoadSettings()
        {
            Settings.Default.Reload();

            var processes = Settings.Default.ProcessExceptions;

            if (processes != null)
            {
                for (var index = 0; index < processes.Count; index++)
                {
                    var process = processes[index];
                    lbProcessExceptions.Items.Add(process);
                }
            }

            nudFramerate.Value = Settings.Default.FramerateLimit;

            chbSelectionRect.Checked = Settings.Default.SelectionRect;
            chbSpawnOnClick.Checked = Settings.Default.SpawnOnClick;
            chbDrawSineWave.Checked = Settings.Default.DrawSineWave;
            chbBurningTaskBar.Checked = Settings.Default.BurningTaskBar;

            _loaded = true;
        }

        private void SaveSettings()
        {
            if (!_loaded)
                return;

            Settings.Default.ProcessExceptions?.Clear();

            for (var index = 0; index < lbProcessExceptions.Items.Count; index++)
            {
                var item = lbProcessExceptions.Items[index];

                Settings.Default.ProcessExceptions.Add((string)item);
            }

            Settings.Default.FramerateLimit = (int)nudFramerate.Value;

            Settings.Default.SelectionRect = chbSelectionRect.Checked;
            Settings.Default.SpawnOnClick = chbSpawnOnClick.Checked;
            Settings.Default.DrawSineWave = chbDrawSineWave.Checked;
            Settings.Default.BurningTaskBar = chbBurningTaskBar.Checked;

            Settings.Default.Save();
        }

        private void Chb_CheckedChanged(object sender, EventArgs e)
        {
            SaveSettings();

            if (sender == chbSelectionRect)
            {
                Task.Run(() =>
                {
                    Registry.SetValue(
                        "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                        "ListviewAlphaSelect", Settings.Default.SelectionRect ? 0 : 1);

                    WindowUtil.RefreshExplorer();
                });
            }
        }

        private void LbProcessExceptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lbProcessExceptions.SelectedItem is string s)
            {
                var result = MessageBox.Show($"Delete '{s}'?", "Delete", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    lbProcessExceptions.Items.Remove(lbProcessExceptions.SelectedItem);
                    SaveSettings();
                }
            }
        }

        private void LbProcessExceptions_DoubleClick(object sender, EventArgs e)
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

        private void NudFramerate_ValueChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }
    }
}