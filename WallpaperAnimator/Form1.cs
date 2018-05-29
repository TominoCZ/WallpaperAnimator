using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WindowUtils;

namespace WallpaperAnimator
{
    public partial class Form1 : Form
    {
        private double _angle;
        private Screen _screen;
        private SolidBrush _brush;

        public Form1()
        {
            InitializeComponent();

            CreateResetWallpaperScritp();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _brush = new SolidBrush(Color.Black);
            _screen = Screen.FromPoint(Location);
            
            ClientSize = _screen.Bounds.Size;
            Location = new Point();

            this.SetAsWallpaper();

            new Thread(() =>
            {
                var invoker = new MethodInvoker(() =>
                {
                    Invalidate();
                    for (var index = 0; index < Controls.Count; index++)
                    {
                        Control control = Controls[index];
                        control.Invalidate();
                    }
                });
                try
                {
                    var span = TimeSpan.FromMilliseconds(16);

                    while (true)
                    {
                        //var now = DateTime.Now;

                        if (IsHandleCreated && Created)
                        {
                            _angle -= 1.5;

                            Invoke(invoker);
                        }

                        //var renderTime = DateTime.Now - now;
                        //var sleepTime = span - renderTime;

                        //if (sleepTime > TimeSpan.Zero)
                        Thread.Sleep(span);//sleepTime);
                    }
                }
                catch { }
            })
            { IsBackground = true }.Start();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < 16; i++)
            {
                DrawSine(e.Graphics, 25, 25, 0.5f, _screen.WorkingArea.Height, i * 5);
            }
        }

        private void DrawSine(Graphics g, float pointWidth, float pointHeight, float sineHeightRatio, float canvasHeight, float angleOffset = 0, float waveLengthRatio = 1)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var sizeY = (canvasHeight * sineHeightRatio - pointWidth) / 2f;

            var steps = ClientSize.Width / pointWidth;

            for (int x = 0; x < steps + 2; x++)
            {
                var progress = x / steps * 360 / waveLengthRatio;

                var angle = progress + _angle + angleOffset;

                var y = (float)Math.Sin(Math.PI / 180 * angle) * sizeY + sizeY;

                var X = x * pointWidth;
                var Y = y + (canvasHeight - canvasHeight * sineHeightRatio) / 2f;

                _brush.Color = Hue(angle);
                g.FillRectangle(_brush, X, Y, pointWidth, pointHeight);
            }
        }

        private Color Hue(double value)
        {
            var rad = Math.PI / 180 * value;
            var third = Math.PI / 3;

            var r = (int)(Math.Sin(rad) * 127 + 128);
            var g = (int)(Math.Sin(rad + third * 2) * 127 + 128);
            var b = (int)(Math.Sin(rad + third * 4) * 127 + 128);

            return Color.FromArgb(r, g, b);
        }

        private void CreateResetWallpaperScritp()
        {
            File.WriteAllLines("RestoreWallpaper.bat", new[]{
                "@echo off",
                "title reset explorer.exe",
                "setlocal",
                "color A",
                "SET /P AREYOUSURE=This will close all your currently opened folders. Are you sure (y/n)?",
                "IF /I \"%AREYOUSURE%\" NEQ \"y\" GOTO END",
                "echo Resetting explorer.exe..",
                "taskkill /f /im explorer.exe",
                "start explorer.exe",
                "pause",
                ":END"});
            /*
            try
            {
                using (var theCurrentMachine = Registry.CurrentUser)
                {
                    using (var theControlPanel = theCurrentMachine.OpenSubKey("Control Panel"))
                    {
                        using (var theDesktop = theControlPanel?.OpenSubKey("Desktop"))
                        {
                            var wp = Convert.ToString(theDesktop?.GetValue("Wallpaper"));

                            
                        }
                    }
                }
            }
            catch { }*/
        }
    }
}
