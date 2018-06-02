using System;
using System.IO;
using System.Windows.Forms;

namespace WallpaperAnimator
{
    public partial class AddProcessForm : Form
    {
        public string ProcessName;

        public AddProcessForm()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void TbProcessName_TextChanged(object sender, EventArgs e)
        {
            var pn = Path.GetFileNameWithoutExtension(tbProcessName.Text.Trim());

            if (btnAdd.Enabled = pn.Length > 0)
                ProcessName = pn;
        }

        private void TbProcessName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnAdd.PerformClick();
        }
    }
}