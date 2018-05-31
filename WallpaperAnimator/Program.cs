using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using WindowUtils;
using Math = System.Math;
using Mouse = System.Windows.Input.Mouse;

namespace WallpaperAnimator
{
    internal static class Program
    {
        private static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            using (var w = new Game())
            {
                w.Run(20);
            }
        }
    }

    class Game : GameWindow
    {
        [DllImport("user32.dll")]
        private static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetClassName(int hWnd, StringBuilder lpClassName, int nMaxCount);

        public static Game Instance;
        public static Random Random = new Random();

        private static readonly IKeyboardMouseEvents _events = Hook.GlobalEvents();

        private ParticleManager _particleManager;

        private StringBuilder _className;
        private Point _lastMouse;
        private Size _lastWindowSize;
        private bool _mouseDown;
        private float _angle, _prevAngle;

        private Stopwatch _updateTimer;

        private static GraphicsMode _gMode = new GraphicsMode(32, 16, 0, 0, 0, 2, false);

        private static List<MouseButtons> _down = new List<MouseButtons>();

        public Game() : base(1, 1, _gMode, "", GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {
            Instance = this;

            WindowState = WindowState.Maximized;
            WindowBorder = WindowBorder.Hidden;

            TargetRenderFrequency = 60;

            this.SetAsWallpaper();

            Init();
        }

        private void Init()
        {
            _updateTimer = new Stopwatch();
            _particleManager = new ParticleManager();
            _className = new StringBuilder(256);

            _events.MouseDown += (o, e) =>
            {
                if (!_down.Contains(e.Button))
                    _down.Add(e.Button);

                //if (!IsDesktopFocused())
                //return;

                //MouseButton btn = e.Button == MouseButtons.Right ? MouseButton.Right : MouseButton.Left;

                //OnMouseDown(new MouseButtonEventArgs(e.X, e.Y, btn, true));
            };
            _events.MouseUp += (o, e) =>
            {
                // if (!IsDesktopFocused())
                // return;

                _down.Remove(e.Button);

                // MouseButton btn = e.Button == MouseButtons.Right ? MouseButton.Right : MouseButton.Left;

                //OnMouseUp(new MouseButtonEventArgs(e.X, e.Y, btn, true));
            };
            _events.MouseMove += (o, e) =>
            {
                if (!IsDesktopFocused())
                    return;

                OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
            };
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.CullFace(CullFaceMode.Back);
            GL.LineWidth(1.5f);

            var deltaTime = (float)_updateTimer.Elapsed.TotalMilliseconds / 50;

            DrawSine(20, 20, 0.5f, Height, deltaTime);

            if (_down.Contains(MouseButtons.Left) && IsDesktopFocused())
                SpawnParticles(_lastMouse, 2);

            _particleManager.Render(deltaTime);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _prevAngle = _angle;
            _angle -= 4f;

            if (_lastWindowSize != Size)
            {
                GL.Viewport(0, 0, Width, Height);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                GL.Ortho(0, Width, Height, 0, -1, 1);

                _lastWindowSize = Size;
            }

            if (_mouseDown)
                SpawnParticles(_lastMouse, 4);

            _particleManager.Update();
            _updateTimer.Restart();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            _lastMouse = e.Position;
        }

        private void DrawSine(float pointWidth, float pointHeight, float sineHeightRatio, float canvasHeight, float deltaTime, float phaseOffset = 0, float waveLengthRatio = 1)
        {
            var sizeY = (canvasHeight * sineHeightRatio - pointWidth) / 2f;

            var steps = ClientSize.Width / pointWidth;

            var deltaAngle = _prevAngle + (_angle - _prevAngle) * deltaTime + phaseOffset;

            for (int x = 0; x < steps; x++)
            {
                var progress = x / steps * 360 / waveLengthRatio;

                var angle = progress + deltaAngle;

                var y = (float)Math.Sin(Math.PI / 180 * angle) * sizeY + sizeY;

                var X = x * pointWidth;
                var Y = y + (canvasHeight - canvasHeight * sineHeightRatio) / 2f;

                var c = Hue.Create(angle);

                GL.PushMatrix();

                GL.Translate(X, Y, 0);
                GL.Scale(pointWidth, pointHeight, 0);

                GL.Begin(PrimitiveType.Quads);

                GL.Color3(c.X, c.Y, c.Z);
                GL.Vertex2(0, 0);
                GL.Vertex2(0, 1);
                GL.Vertex2(1, 1);
                GL.Vertex2(1, 0);

                GL.End();

                GL.PopMatrix();
            }
        }

        private void SpawnParticles(Point p, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var offX = -25 + (float)Random.NextDouble() * 50;
                var offY = -25 + (float)Random.NextDouble() * 50;

                var dir = Vector2.Normalize(new Vector2(offX, offY)) * 4;

                Particle particle;

                var r = Random.Next(1, 4);

                if (r == 1)
                {
                    particle = new ParticleTriangle(p.X + offX, p.Y + offY, dir.X, dir.Y, Random.Next(15, 25),
                            8 + (float)Random.NextDouble() * 24)
                    { Traction = 1.15f };
                }
                else if (r == 2)
                {
                    particle = new ParticleSquare(p.X + offX, p.Y + offY, dir.X, dir.Y, Random.Next(15, 25),
                            8 + (float)Random.NextDouble() * 24)
                    { Traction = 1.15f };
                }
                else
                {
                    particle = new ParticleCircle(p.X + offX, p.Y + offY, dir.X, dir.Y, Random.Next(15, 25),
                            4 + (float)Random.NextDouble() * 20)
                    { Traction = 1.15f };
                }

                _particleManager.AddParticle(particle);
            }
        }

        private bool IsDesktopFocused()
        {
            _className.Clear();

            if (GetClassName(GetForegroundWindow(), _className, 256) > 0)
            {
                string cName = _className.ToString();
                return cName == "Progman" || cName == "WorkerW";
            }

            return false;
        }
    }

    class ParticleManager
    {
        private readonly List<Particle> _particles = new List<Particle>();

        public void AddParticle(Particle particle)
        {
            _particles.Add(particle);
        }

        public void Update()
        {
            for (var index = _particles.Count - 1; index >= 0; index--)
            {
                var particle = _particles[index];

                particle.Update();

                if (particle.IsDead)
                    _particles.Remove(particle);
            }
        }

        public void Render(float deltaTime)
        {
            for (var index = 0; index < _particles.Count; index++)
            {
                _particles[index].Render(deltaTime);
            }
        }
    }

    class ParticleSquare : Particle
    {
        public ParticleSquare(float x, float y, float mx, float my, int maxAge, float size) : base(x, y, mx, my, maxAge, size)
        {
            var mult = Math.Min(32 / size, 4);

            Mx *= mult;
            My *= mult;
        }

        public override void Render(float deltaTime)
        {
            var deltaX = PrevX + (X - PrevX) * deltaTime;
            var deltaY = PrevY + (Y - PrevY) * deltaTime;

            var deltaSize = PrevSize + (Size - PrevSize) * deltaTime;
            var deltaAlpha = Math.Min(1, Math.Max(0, PrevAlpha + (Alpha - PrevAlpha) * deltaTime));

            var deltaAngle = PrevAngle + (Angle - PrevAngle) * deltaTime;

            var c = Hue.Create(deltaX / Game.Instance.Width * 360);

            GL.PushMatrix();

            GL.Translate(deltaX, deltaY, 0);
            GL.Rotate(deltaAngle, 0, 0, 1);
            GL.Scale(deltaSize, deltaSize, 0);

            GL.Begin(PrimitiveType.LineLoop);

            GL.Color4(c.X, c.Y, c.Z, deltaAlpha);
            GL.Vertex2(-0.5, -0.5);
            GL.Vertex2(-0.5, 0.5);
            GL.Vertex2(0.5, 0.5);
            GL.Vertex2(0.5, -0.5);

            GL.End();
            GL.PopMatrix();
        }
    }

    class ParticleTriangle : Particle
    {
        public ParticleTriangle(float x, float y, float mx, float my, int maxAge, float size) : base(x, y, mx, my, maxAge, size)
        {
            var mult = Math.Min(32 / size, 4);

            Mx *= mult;
            My *= mult;
        }

        public override void Render(float deltaTime)
        {
            var deltaX = PrevX + (X - PrevX) * deltaTime;
            var deltaY = PrevY + (Y - PrevY) * deltaTime;

            var deltaSize = PrevSize + (Size - PrevSize) * deltaTime;
            var deltaAlpha = Math.Min(1, Math.Max(0, PrevAlpha + (Alpha - PrevAlpha) * deltaTime));

            var deltaAngle = PrevAngle + (Angle - PrevAngle) * deltaTime;

            var c = Hue.Create(deltaX / Game.Instance.Width * 360);

            GL.PushMatrix();

            GL.Translate(deltaX, deltaY, 0);
            GL.Rotate(deltaAngle, 0, 0, 1);
            GL.Scale(deltaSize, deltaSize, 0);

            GL.Begin(PrimitiveType.LineLoop);

            var x = 0.43301270189222D;

            GL.Color4(c.X, c.Y, c.Z, deltaAlpha);
            GL.Vertex2(0, -x);
            GL.Vertex2(-0.5, x);
            GL.Vertex2(0.5, x);

            GL.End();
            GL.PopMatrix();
        }
    }

    class ParticleCircle : Particle
    {
        public ParticleCircle(float x, float y, float mx, float my, int maxAge, float size) : base(x, y, mx, my, maxAge, size)
        {
            var mult = Math.Min(32 / size, 4);

            Mx *= mult;
            My *= mult;
        }

        public override void Render(float deltaTime)
        {
            var deltaX = PrevX + (X - PrevX) * deltaTime;
            var deltaY = PrevY + (Y - PrevY) * deltaTime;

            var deltaSize = PrevSize + (Size - PrevSize) * deltaTime;
            var deltaAlpha = Math.Min(1, Math.Max(0, PrevAlpha + (Alpha - PrevAlpha) * deltaTime));

            var deltaAngle = PrevAngle + (Angle - PrevAngle) * deltaTime;

            var c = Hue.Create(deltaX / Game.Instance.Width * 360);

            GL.PushMatrix();

            GL.Translate(deltaX, deltaY, 0);
            GL.Rotate(deltaAngle, 0, 0, 1);
            GL.Scale(deltaSize, deltaSize, 0);

            GL.Begin(PrimitiveType.LineLoop);

            GL.Color4(c.X, c.Y, c.Z, deltaAlpha);

            for (int i = 0; i < 24; i++)
            {
                var a = i / 24f;

                var x = Math.Cos(a * MathHelper.TwoPi) / 2;
                var y = Math.Sin(a * MathHelper.TwoPi) / 2;

                GL.Vertex2(x, y);
            }

            GL.End();
            GL.PopMatrix();
        }
    }

    class Particle
    {
        public float X, Y, PrevX, PrevY;

        public float Mx, My;

        public int Age, MaxAge;

        public float Size, PrevSize, StartSize;

        public float Alpha, PrevAlpha;

        public float Angle, PrevAngle;

        public float Traction = 1;

        protected int _direction;

        public bool IsDead;

        protected Particle(float x, float y, float mx, float my, int maxAge, float size)
        {
            PrevX = X = x;
            PrevY = Y = y;

            Mx = mx;
            My = my;

            MaxAge = maxAge;
            StartSize = PrevSize = Size = size;

            PrevAlpha = Alpha = 1;

            PrevAngle = Angle = (float)Game.Random.NextDouble() * 360;

            _direction = Game.Random.NextDouble() >= 0.5 ? -1 : 1;
        }

        public virtual void Update()
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

            Mx *= 0.9875f / Traction;
            My *= 0.9875f / Traction;
        }

        public virtual void Render(float deltaTime)
        {
        }
    }

    static class Hue
    {
        public static Vector3 Create(double value)
        {
            var rad = Math.PI / 180 * value;
            var third = Math.PI / 3;

            var r = (float)Math.Sin(rad) * 0.5f + 0.5f;
            var g = (float)Math.Sin(rad + third * 2) * 0.5f + 0.5f;
            var b = (float)Math.Sin(rad + third * 4) * 0.5f + 0.5f;

            return new Vector3(r, g, b);
        }
    }

    public static class WindowUtil
    {
        public static void SetAsWallpaper(this GameWindow form)
        {
            W32.SendMessageTimeout(W32.FindWindow("Progman", null), 0x052C, new IntPtr(0), IntPtr.Zero, W32.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out var result);
            //W32.SendMessageTimeout(W32.FindWindow("Progman", null), 0, new IntPtr(0), IntPtr.Zero, W32.SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out var result);

            IntPtr workerw = IntPtr.Zero;
            W32.EnumWindows((tophandle, topparamhandle) =>
            {
                if (W32.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero) != IntPtr.Zero)
                    workerw = W32.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);

                return true;
            }, IntPtr.Zero);

            IntPtr dc = W32.GetDCEx(workerw, IntPtr.Zero, (W32.DeviceContextValues)0x403);
            W32.ReleaseDC(workerw, dc);

            if (W32.GetParent(form.WindowInfo.Handle) != workerw)
            {
                form.Location = PointToWallpaper(form.Location);

                W32.SetParent(form.WindowInfo.Handle, workerw);
            }
        }

        public static void SetAsDesktopWindow(this GameWindow form)
        {
            IntPtr window = W32.GetDesktopWindow();

            if (W32.GetParent(form.WindowInfo.Handle) == window)
                return;

            var p = form.Location;

            W32.SetParent(form.WindowInfo.Handle, window);

            form.Location = p;
        }

        /// <summary>
        /// Converts a screen Point to a Point on the wallpaper.
        /// This is due to the different start points between these layers.
        /// Screen(Desktop) position is relative to your main monitor's start point(0,0), while the position on the wallpaper is relative to the left-most monitor's start point
        /// </summary>
        /// <returns></returns>
        private static Point PointToWallpaper(Point p)
        {
            var offsetX = 0;
            var offsetY = 0;

            foreach (var s in Screen.AllScreens)
            {
                if (s.Bounds.Location.X <= p.X)
                    offsetX -= s.Bounds.Location.X;
                if (s.Bounds.Location.Y <= p.Y)
                    offsetY -= s.Bounds.Location.Y;
            }

            p.Offset(offsetX, offsetY);

            return p;
        }
    }
}