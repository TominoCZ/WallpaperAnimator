using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowUtils;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        public Form1()
        {
            InitializeComponent();

            var sb = new StringBuilder(256);
            var w = W32.GetDesktopWindow();
            var target = IntPtr.Zero;

            W32.EnumChildWindows(w, (hwnd, param) =>
            {
                sb.Clear();

                if (W32.GetClassName(hwnd, sb, 256) > 0)
                {
                    if (sb.ToString() == "SysListView32")
                    {
                        target = hwnd;
                    }
                }

                return true;
            }, IntPtr.Zero);

            IntPtr dc = W32.GetDCEx(target, IntPtr.Zero, (W32.DeviceContextValues)0x403);

            new Thread(() =>
            {
                var bg = BufferedGraphicsManager.Current;
                var g = bg.Allocate(dc, Screen.AllScreens[0].Bounds);

                while (true)
                {
                    //g.Clear(Color.FromArgb(0, 0, 0, 0));
                    g.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 0, 0)), 0, 0, 100, 100);
                    
                    bg.Invalidate();

                    Thread.Sleep(16);
                }

                //W32.ReleaseDC(window, dc);
            })
            { IsBackground = true }.Start();
        }
    }
}
