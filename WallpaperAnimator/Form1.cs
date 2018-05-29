using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WindowUtils;

namespace WallpaperAnimator
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetClassName(int hWnd, StringBuilder lpClassName, int nMaxCount);

        private MethodInfo _onClick = typeof(Control).GetRuntimeMethods().Single(m => m.Name == "OnClick");
        private IKeyboardMouseEvents _events = Hook.GlobalEvents();

        public static Screen Screen;
        public static Random Random = new Random();

        private Point _lastMouse;
        private SolidBrush _brush;
        private double _angle;
        private bool _mouseDown;

        private DateTime _lastUpdateTime = DateTime.Now;

        private List<Particle> _particles = new List<Particle>();

        private bool DesktopFocus
        {
            get
            {
                var className = new StringBuilder(256);

                if (GetClassName(GetForegroundWindow(), className, 256) > 0)
                {
                    string cName = className.ToString();
                    return cName == "Progman" || cName == "WorkerW";
                }

                return false;
            }
        }

        public Form1()
        {
            InitializeComponent();

            void Click(MouseEventArgs e)
            {
                if (DesktopFocus)
                {
                    var clickedControl = false;

                    for (var index = 0; index < Controls.Count; index++)
                    {
                        Control control = Controls[index];

                        if (new Rectangle(control.Location, control.ClientSize).Contains(e.Location))
                        {
                            clickedControl = true;
                            if (control is Button btn)
                                btn.PerformClick();
                            else
                                _onClick.Invoke(control, new object[] { e });
                        }
                    }

                    if (!clickedControl)
                        OnMouseClick(e);
                }
            }

            var wasDown = false;

            _events.MouseDown += (o, e) =>
            {
                wasDown = true;

                if (DesktopFocus)
                {
                    OnMouseDown(e);
                }
            };
            _events.MouseUp += (o, e) =>
            {
                if (wasDown)
                    Click(e);

                wasDown = false;

                if (DesktopFocus)
                {
                    OnMouseUp(e);
                }
            };
            _events.MouseMove += (o, e) =>
            {
                if (DesktopFocus)
                {
                    Focus();
                    OnMouseMove(e);
                }
            };

            CreateResetWallpaperScritp();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _brush = new SolidBrush(Color.Black);
            Screen = Screen.FromPoint(Location);

            ClientSize = Screen.Bounds.Size;
            Location = Screen.Bounds.Location;

            this.SetAsWallpaper();

            new Thread(() =>
                {
                    try
                    {
                        var span = TimeSpan.FromMilliseconds(16);

                        while (true)
                        {
                            //var now = DateTime.Now;

                            if (IsHandleCreated && Created)
                            {
                                _angle -= 1.5;

                                Invoke((MethodInvoker)(() =>
                               {
                                   Invalidate();
                                   for (var index = 0; index < Controls.Count; index++)
                                   {
                                       Controls[index].Invalidate();
                                   }
                               }));
                            }

                            //var renderTime = DateTime.Now - now;
                            //var sleepTime = span - renderTime;

                            //if (sleepTime > TimeSpan.Zero)
                            Thread.Sleep(span); //sleepTime);
                        }
                    }
                    catch
                    {
                    }
                })
            { IsBackground = true }.Start();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
           //
           // if (e.Button != MouseButtons.Left)
               // return;

           // SpawnParticles(e.Location, 16);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _mouseDown = true;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _mouseDown = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            _lastMouse = e.Location;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var now = DateTime.Now;
            var deltaTime = (float)(now - _lastUpdateTime).TotalMilliseconds / 50;

            if (deltaTime >= 1)
            {
                OnUpdate();

                _lastUpdateTime = now;

                deltaTime--;
            }

            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            // for (int i = 0; i < 16; i++)
            // {
            DrawSine(e.Graphics, 25, 25, 0.5f, Screen.WorkingArea.Height);
            // }
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            OnRender(e.Graphics, deltaTime);
        }

        private void DrawSine(Graphics g, float pointWidth, float pointHeight, float sineHeightRatio, float canvasHeight, float angleOffset = 0, float waveLengthRatio = 1)
        {
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

        private void SpawnParticles(Point p, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var offX = -25 + (float)Random.NextDouble() * 50;
                var offY = -25 + (float)Random.NextDouble() * 50;

                var dir = Vector2.Normalize(new Vector2(offX, offY)) * 4;

                _particles.Add(new Particle(p.X + offX, p.Y + offY, dir.X, dir.Y, Random.Next(15, 25), 8 + (float)Random.NextDouble() * 24));
            }
        }

        private void OnUpdate()
        {
            for (var index = _particles.Count - 1; index >= 0; index--)
            {
                var particle = _particles[index];
                particle.Update();

                if (particle.IsDead)
                    _particles.Remove(particle);
            }
        }

        private void OnRender(Graphics g, float deltaTime)
        {
            if (_mouseDown && DesktopFocus)
                SpawnParticles(_lastMouse, 1);

            for (var index = 0; index < _particles.Count; index++)
            {
                _particles[index].Render(g, deltaTime);
            }
        }

        public static Color Hue(double value)
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

    internal class Particle
    {
        public float X, Y, PrevX, PrevY;

        public float Mx, My;

        public int Age, MaxAge;

        public float Size, PrevSize, StartSize;

        public float Alpha, PrevAlpha;

        public float Angle, PrevAngle;

        private readonly int _direction;

        public bool IsDead;

        private SolidBrush _brush = new SolidBrush(Color.Black);

        public Particle(float x, float y, float mx, float my, int maxAge, float size)
        {
            PrevX = X = x;
            PrevY = Y = y;

            var mult = Math.Min(32 / size, 5);

            Mx = mx * mult;
            My = my * mult;

            MaxAge = maxAge;
            StartSize = PrevSize = Size = size;

            PrevAlpha = Alpha = 1;

            PrevAngle = Angle = (float)Form1.Random.NextDouble() * 45;

            _direction = Form1.Random.NextDouble() >= 0.5 ? -1 : 1;
        }

        public void Update()
        {
            PrevX = X;
            PrevY = Y;

            PrevSize = Size;
            PrevAlpha = Alpha;

            PrevAngle = Angle;

            if (Age++ >= MaxAge)
            {
                if (Size <= 1)
                    IsDead = true;
                else
                {
                    Size *= 0.75f;
                    Alpha *= Size / StartSize;
                }
            }

            var mult = (float)Math.Min(Math.Sqrt(Mx * Mx + My * My), 4) * 4 * _direction;

            Angle += mult;

            X += Mx;
            Y += My;

            Mx *= 0.95f;
            My *= 0.95f;
        }

        public void Render(Graphics g, float deltaTime)
        {
            var deltaX = PrevX + (X - PrevX) * deltaTime;
            var deltaY = PrevY + (Y - PrevY) * deltaTime;

            var deltaSize = PrevSize + (Size - PrevSize) * deltaTime;
            var deltaAlpha = PrevAlpha + (Alpha - PrevAlpha) * deltaTime;

            var deltaAngle = PrevAngle + (Angle - PrevAngle) * deltaTime;

            var c = Form1.Hue(deltaX / Form1.Screen.Bounds.Width * 360);

            _brush.Color = Color.FromArgb((int)(255 * deltaAlpha), c.R, c.G, c.B);

            g.TranslateTransform(deltaX, deltaY);
            g.RotateTransform(deltaAngle);
            g.FillRectangle(_brush, -deltaSize / 2, -deltaSize / 2, deltaSize, deltaSize);
            g.RotateTransform(-deltaAngle);
            g.TranslateTransform(-deltaX, -deltaY);
        }
    }
}