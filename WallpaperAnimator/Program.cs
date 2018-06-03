using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WallpaperAnimator.Properties;
using WindowUtils;
using Math = System.Math;

namespace WallpaperAnimator
{
    internal static class Program
    {
        private static void Main()
        {
            var game = new Game();
            Settings.Default.SettingsLoaded += game.LoadSettings;

            WindowUtil.RefreshExplorer();

            game.Run(20);
        }
    }

    public class SystemTrayApp : ApplicationContext
    {
        public EventHandler OnExit;

        private bool _lastActive = true;
        private NotifyIcon _trayIcon;
        private Form _configForm;

        public SystemTrayApp()
        {
            // Initialize Tray Icon
            _trayIcon = new NotifyIcon()
            {
                Icon = Resources.icon_on,
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("Settings", OpenSettings),
                    new MenuItem("Close", Exit)
                }),
                Visible = true
            };

            _trayIcon.DoubleClick += OpenSettings;
        }

        public void Hide()
        {
            _trayIcon.Visible = false;
        }

        public void SetActive(bool active)
        {
            if (_lastActive == active)
                return;

            _trayIcon.Icon = (_lastActive = active) ? Resources.icon_on : Resources.icon_off;
            ;
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            if (_configForm == null || _configForm.IsDisposed || _configForm.Disposing)
            {
                _configForm = new ConfigForm();
                _configForm.Show();
            }
        }

        public void Exit(object sender, EventArgs e)
        {
            // Hide tray icon_on, otherwise it will remain shown until user mouses over it
            Hide();

            if (_configForm != null && !_configForm.IsDisposed && !_configForm.Disposing)
            {
                _configForm.Close();
            }

            OnExit(this, null);

            Application.Exit();
        }
    }

    internal class Game : GameWindow
    {
        public static Game Instance;
        public static Random Random = new Random();

        private static readonly IKeyboardMouseEvents _events = Hook.GlobalEvents();

        private ParticleManager _particleManager;

        private StringBuilder _className;
        private Screen _startScreen;
        private Point _lastMouse;
        private Point _lastMouseDown;
        private Size _lastWindowSize;
        private long _checkTicks;
        private float _angle, _prevAngle;
        private bool _canUpdate = true;
        private bool _closing;
        private bool _desktopFocused;

        private Stopwatch _updateTimer;

        private static GraphicsMode _gMode = new GraphicsMode(32, 16, 0, 8, 0, 2, false);
        private static StringCollection _processExceptions = new StringCollection();
        private static SystemTrayApp _trayIcon;
        private static List<MouseButtons> _down = new List<MouseButtons>();

        /*
        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;*/

        public Game() : base(1, 1, _gMode, "", GameWindowFlags.Default, DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            Instance = this;

            VSync = VSyncMode.On;

            WindowState = WindowState.Maximized;
            WindowBorder = WindowBorder.Hidden;

            _startScreen = Screen.FromPoint(Location);

            this.SetAsWallpaper();

            //SetWindowLong(WindowInfo.Handle, GWL_EXSTYLE, GetWindowLong(WindowInfo.Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
            //SetLayeredWindowAttributes(WindowInfo.Handle, 0, 128, LWA_ALPHA);
            //SetLayeredWindowAttributes(WindowInfo.Handle, 0, 0, LWA_COLORKEY);

            Init();

            //GetInfo.Test();
        }

        private void Init()
        {
            InitTrayIcon();

            _updateTimer = new Stopwatch();
            _particleManager = new ParticleManager();
            _className = new StringBuilder(256);

            _events.MouseDown += (o, e) =>
            {
                if (!_down.Contains(e.Button))
                {
                    _lastMouseDown = e.Location;

                    _down.Add(e.Button);
                }

                if (!(_desktopFocused = IsDesktopFocused()))
                    return;

                MouseButton btn = e.Button == MouseButtons.Right ? MouseButton.Right : MouseButton.Left;

                OnMouseDown(new MouseButtonEventArgs(e.X, e.Y, btn, true));
            };
            _events.MouseUp += (o, e) =>
            {
                _down.Remove(e.Button);

                _desktopFocused = IsDesktopFocused();

                MouseButton btn = e.Button == MouseButtons.Right ? MouseButton.Right : MouseButton.Left;

                OnMouseUp(new MouseButtonEventArgs(e.X, e.Y, btn, true));
            };
            _events.MouseMove += (o, e) =>
            {
                if (Settings.Default.SelectionRect && _down.Contains(MouseButtons.Left) && _desktopFocused)
                {
                    var sb = new StringBuilder(256);
                    var w = W32.GetDesktopWindow();
                    var target = IntPtr.Zero;

                    W32.EnumChildWindows(w, (hwnd, param) =>
                    {
                        if (W32.GetClassName(hwnd, sb, 256) > 0)
                        {
                            if (sb.ToString() == "SysListView32")
                            {
                                target = hwnd;
                            }
                        }

                        sb.Clear();

                        return true;
                    }, IntPtr.Zero);

                    if (target != IntPtr.Zero)
                        W32.SendMessage(target, 0x001F, 0, IntPtr.Zero);
                }

                _lastMouse = e.Location;
            };
        }

        private void InitTrayIcon()
        {
            Task.Run(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _trayIcon = new SystemTrayApp();

                _trayIcon.OnExit += (o, e) =>
                {
                    Task.Run(() =>
                    {
                        _closing = true;

                        Visible = false;

                        OnClosing(new CancelEventArgs());
                    });
                };

                Application.Run(_trayIcon);
            });
        }

        public void LoadSettings(object sender, EventArgs args)
        {
            if (Settings.Default.ProcessExceptions is StringCollection s)
                _processExceptions = s;

            TargetRenderFrequency = Settings.Default.FramerateLimit;

            if (!Settings.Default.SelectionRect)
            {
                Task.Run(() =>
                {
                    Registry.SetValue(
                        "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                        "ListviewAlphaSelect", 0);

                    WindowUtil.RefreshExplorer();
                });
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (!_canUpdate || _closing)
                return;

            _desktopFocused = IsDesktopFocused();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.ClearColor(0, 0, 0, 0);
            //GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            //GL.CullFace(CullFaceMode.Back);
            GL.LineWidth(1f);

            var deltaTime = (float)_updateTimer.Elapsed.TotalMilliseconds / 50;

            if (Settings.Default.DrawSineWave)
                DrawSine(25, 25, 0.5f, Height, deltaTime);

            _particleManager.Render(deltaTime);

            var mouseDown = _down.Contains(MouseButtons.Left);

            if (mouseDown)
                _desktopFocused = IsDesktopFocused();

            if (mouseDown && _desktopFocused)
            {
                if (Settings.Default.SpawnOnClick)
                    SpawnParticles(_lastMouse, 2);

                if (Settings.Default.SelectionRect)
                {
                    var color1 = Hue.Create(_lastMouseDown.X / (float)Width * 360);
                    var color2 = Hue.Create(_lastMouse.X / (float)Width * 360);

                    GL.LineWidth(2f);

                    GL.Begin(PrimitiveType.Polygon);
                    //filled rectangle
                    GL.Color4(color1.X, color1.Y, color1.Z, .2f);
                    GL.Vertex2(_lastMouseDown.X, _lastMouseDown.Y);
                    GL.Vertex2(_lastMouseDown.X, _lastMouse.Y);

                    GL.Color4(color2.X, color2.Y, color2.Z, .2f);
                    GL.Vertex2(_lastMouse.X, _lastMouse.Y);
                    GL.Vertex2(_lastMouse.X, _lastMouseDown.Y);
                    GL.End();

                    GL.Begin(PrimitiveType.LineLoop);

                    //outline
                    GL.Color4(color1.X, color1.Y, color1.Z, 1f);
                    GL.Vertex2(_lastMouseDown.X, _lastMouseDown.Y);
                    GL.Vertex2(_lastMouseDown.X, _lastMouse.Y);

                    GL.Color4(color2.X, color2.Y, color2.Z, 1f);
                    GL.Vertex2(_lastMouse.X, _lastMouse.Y);
                    GL.Vertex2(_lastMouse.X, _lastMouseDown.Y);
                    GL.End();

                    //the filled circles
                    GL.Color4(color1.X, color1.Y, color1.Z, 1f);

                    GL.Begin(PrimitiveType.Polygon);
                    VertexUtil.PutCircle(_lastMouseDown.X, _lastMouseDown.Y, 6, 16);
                    GL.End();

                    GL.Begin(PrimitiveType.Polygon);
                    VertexUtil.PutCircle(_lastMouseDown.X, _lastMouse.Y, 6, 16);
                    GL.End();

                    GL.Color4(color2.X, color2.Y, color2.Z, 1f);

                    GL.Begin(PrimitiveType.Polygon);
                    VertexUtil.PutCircle(_lastMouse.X, _lastMouse.Y, 6, 16);
                    GL.End();

                    GL.Begin(PrimitiveType.Polygon);
                    VertexUtil.PutCircle(_lastMouse.X, _lastMouseDown.Y, 6, 16);
                    GL.End();
                }
            }

            SwapBuffers();
            
            //glEnable GL_ALPHA_TEST 

            //glEnable GL_COLOR_MATERIAL 

            //Transparent-OpenGL-window 
            //glBlendFunc GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA 
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_closing)
                return;

            _desktopFocused = IsDesktopFocused();

            if (_checkTicks++ >= 10)
            {
                if (!(_canUpdate = CanUpdate()))
                {
                    _trayIcon?.SetActive(false);
                    return;
                }

                _checkTicks = 0;
            }

            _trayIcon?.SetActive(true);
            TargetRenderFrequency = Settings.Default.FramerateLimit;

            //check screen size
            if (_lastWindowSize != Size)
            {
                GL.Viewport(0, 0, Width, Height);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                GL.Ortho(0, Width, Height, 0, -1, 1);

                _lastWindowSize = Size;
            }

            _prevAngle = _angle;
            _angle -= 4f;

            if (Settings.Default.BurningTaskBar)
            {
                var step = Width / 16f;

                for (int i = 0; i <= 16; i++)
                {
                    var x = step * i;

                    SpawnFireParticles(x, Height, 1);
                }
            }

            _particleManager.Update();
            _updateTimer.Restart();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Registry.SetValue(
                "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced",
                "ListviewAlphaSelect", 1);

            WindowUtil.ReloadWallpaper();
            WindowUtil.RefreshExplorer();

            base.OnClosing(e);
        }

        private void DrawSine(float pointWidth, float pointHeight, float sineHeightRatio, float canvasHeight,
            float deltaTime, float phaseOffset = 0, float waveLengthRatio = 1)
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

                GL.Begin(PrimitiveType.Polygon);

                GL.Color4(c.X, c.Y, c.Z, 0.2);
                GL.Vertex2(0, 0);
                GL.Vertex2(0, 1);
                GL.Vertex2(1, 1);
                GL.Vertex2(1, 0);

                GL.End();

                GL.Begin(PrimitiveType.LineLoop);

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
                    { Acceleration = 0.9f };
                }
                else if (r == 2)
                {
                    particle = new ParticleSquare(p.X + offX, p.Y + offY, dir.X, dir.Y, Random.Next(15, 25),
                            8 + (float)Random.NextDouble() * 24)
                    { Acceleration = 0.9f };
                }
                else
                {
                    particle = new ParticleCircle(p.X + offX, p.Y + offY, dir.X, dir.Y, Random.Next(15, 25),
                            4 + (float)Random.NextDouble() * 20)
                    { Acceleration = 0.9f };
                }

                _particleManager.AddParticle(particle);
            }
        }

        private void SpawnFireParticles(float x, float y, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var offX = -25 + (float)Random.NextDouble() * 50;
                var offY = -25 + (float)Random.NextDouble() * 50;

                var dir = Vector2.Normalize(new Vector2(offX, offY));

                var size = 8 + (float)Random.NextDouble() * 24;

                var particle = new ParticleSquare(x + offX, y + (float)Math.Sqrt(size * size + size * size), dir.X,
                        -Math.Abs(dir.Y * 2f), Random.Next(20, 40),
                        size)
                { Acceleration = 1f };

                _particleManager.AddParticle(particle);
            }
        }

        private bool IsDesktopFocused()
        {
            _className.Clear();

            if (W32.GetClassName(W32.GetForegroundWindow(), _className, 256) > 0)
            {
                string cName = _className.ToString();
                return cName == "Progman" || cName == "WorkerW";
            }

            return false;
        }

        private bool CanUpdate()
        {
            var b = true;

            if (_processExceptions.Count > 0)
            {
                Process[] processes = Process.GetProcesses(".");

                Parallel.ForEach(processes, p =>
                {
                    for (var index = 0; index < _processExceptions.Count; index++)
                    {
                        var exception = _processExceptions[index];

                        if (string.Equals(p.ProcessName.ToLower(), exception, StringComparison.OrdinalIgnoreCase))
                            b = false;
                    }

                    p.Dispose();
                });

                if (!b)
                    return false;
            }

            W32.EnumWindows((tophandle, topparamhandle) =>
            {
                if (tophandle != IntPtr.Zero && tophandle != WindowInfo.Handle)
                {
                    var placement = WindowUtil.GetPlacement(tophandle);

                    if (placement.showCmd == WindowUtil.ShowWindowCommands.Maximized &&
                        _startScreen.Bounds.Contains(placement.rcNormalPosition.Location))
                    {
                        b = false;
                    }

                    return true;
                }

                return true;
            }, IntPtr.Zero);

            return b;
        }
        /*
        static class GetInfo
        {
            public enum LVM
            {
                FIRST = 0x1000,
                SETUNICODEFORMAT = 0x2005,        // CCM_SETUNICODEFORMAT,
                GETUNICODEFORMAT = 0x2006,        // CCM_GETUNICODEFORMAT,
                GETBKCOLOR = (FIRST + 0),
                SETBKCOLOR = (FIRST + 1),
                GETIMAGELIST = (FIRST + 2),
                SETIMAGELIST = (FIRST + 3),
                GETITEMCOUNT = (FIRST + 4),
                GETITEMA = (FIRST + 5),
                GETITEMW = (FIRST + 75),
                GETITEM = GETITEMW,
                //GETITEM                = GETITEMA,
                SETITEMA = (FIRST + 6),
                SETITEMW = (FIRST + 76),
                SETITEM = SETITEMW,
                //SETITEM                = SETITEMA,
                INSERTITEMA = (FIRST + 7),
                INSERTITEMW = (FIRST + 77),
                INSERTITEM = INSERTITEMW,
                //INSERTITEM             = INSERTITEMA,
                DELETEITEM = (FIRST + 8),
                DELETEALLITEMS = (FIRST + 9),
                GETCALLBACKMASK = (FIRST + 10),
                SETCALLBACKMASK = (FIRST + 11),
                GETNEXTITEM = (FIRST + 12),
                FINDITEMA = (FIRST + 13),
                FINDITEMW = (FIRST + 83),
                GETITEMRECT = (FIRST + 14),
                SETITEMPOSITION = (FIRST + 15),
                GETITEMPOSITION = (FIRST + 16),
                GETSTRINGWIDTHA = (FIRST + 17),
                GETSTRINGWIDTHW = (FIRST + 87),
                HITTEST = (FIRST + 18),
                ENSUREVISIBLE = (FIRST + 19),
                SCROLL = (FIRST + 20),
                REDRAWITEMS = (FIRST + 21),
                ARRANGE = (FIRST + 22),
                EDITLABELA = (FIRST + 23),
                EDITLABELW = (FIRST + 118),
                EDITLABEL = EDITLABELW,
                //EDITLABEL              = EDITLABELA,
                GETEDITCONTROL = (FIRST + 24),
                GETCOLUMNA = (FIRST + 25),
                GETCOLUMNW = (FIRST + 95),
                SETCOLUMNA = (FIRST + 26),
                SETCOLUMNW = (FIRST + 96),
                INSERTCOLUMNA = (FIRST + 27),
                INSERTCOLUMNW = (FIRST + 97),
                DELETECOLUMN = (FIRST + 28),
                GETCOLUMNWIDTH = (FIRST + 29),
                SETCOLUMNWIDTH = (FIRST + 30),
                GETHEADER = (FIRST + 31),
                CREATEDRAGIMAGE = (FIRST + 33),
                GETVIEWRECT = (FIRST + 34),
                GETTEXTCOLOR = (FIRST + 35),
                SETTEXTCOLOR = (FIRST + 36),
                GETTEXTBKCOLOR = (FIRST + 37),
                SETTEXTBKCOLOR = (FIRST + 38),
                GETTOPINDEX = (FIRST + 39),
                GETCOUNTPERPAGE = (FIRST + 40),
                GETORIGIN = (FIRST + 41),
                UPDATE = (FIRST + 42),
                SETITEMSTATE = (FIRST + 43),
                GETITEMSTATE = (FIRST + 44),
                GETITEMTEXTA = (FIRST + 45),
                GETITEMTEXTW = (FIRST + 115),
                SETITEMTEXTA = (FIRST + 46),
                SETITEMTEXTW = (FIRST + 116),
                SETITEMCOUNT = (FIRST + 47),
                SORTITEMS = (FIRST + 48),
                SETITEMPOSITION32 = (FIRST + 49),
                GETSELECTEDCOUNT = (FIRST + 50),
                GETITEMSPACING = (FIRST + 51),
                GETISEARCHSTRINGA = (FIRST + 52),
                GETISEARCHSTRINGW = (FIRST + 117),
                GETISEARCHSTRING = GETISEARCHSTRINGW,
                //GETISEARCHSTRING       = GETISEARCHSTRINGA,
                SETICONSPACING = (FIRST + 53),
                SETEXTENDEDLISTVIEWSTYLE = (FIRST + 54),            // optional wParam == mask
                GETEXTENDEDLISTVIEWSTYLE = (FIRST + 55),
                GETSUBITEMRECT = (FIRST + 56),
                SUBITEMHITTEST = (FIRST + 57),
                SETCOLUMNORDERARRAY = (FIRST + 58),
                GETCOLUMNORDERARRAY = (FIRST + 59),
                SETHOTITEM = (FIRST + 60),
                GETHOTITEM = (FIRST + 61),
                SETHOTCURSOR = (FIRST + 62),
                GETHOTCURSOR = (FIRST + 63),
                APPROXIMATEVIEWRECT = (FIRST + 64),
                SETWORKAREAS = (FIRST + 65),
                GETWORKAREAS = (FIRST + 70),
                GETNUMBEROFWORKAREAS = (FIRST + 73),
                GETSELECTIONMARK = (FIRST + 66),
                SETSELECTIONMARK = (FIRST + 67),
                SETHOVERTIME = (FIRST + 71),
                GETHOVERTIME = (FIRST + 72),
                SETTOOLTIPS = (FIRST + 74),
                GETTOOLTIPS = (FIRST + 78),
                SORTITEMSEX = (FIRST + 81),
                SETBKIMAGEA = (FIRST + 68),
                SETBKIMAGEW = (FIRST + 138),
                GETBKIMAGEA = (FIRST + 69),
                GETBKIMAGEW = (FIRST + 139),
                SETSELECTEDCOLUMN = (FIRST + 140),
                SETVIEW = (FIRST + 142),
                GETVIEW = (FIRST + 143),
                INSERTGROUP = (FIRST + 145),
                SETGROUPINFO = (FIRST + 147),
                GETGROUPINFO = (FIRST + 149),
                REMOVEGROUP = (FIRST + 150),
                MOVEGROUP = (FIRST + 151),
                GETGROUPCOUNT = (FIRST + 152),
                GETGROUPINFOBYINDEX = (FIRST + 153),
                MOVEITEMTOGROUP = (FIRST + 154),
                GETGROUPRECT = (FIRST + 98),
                SETGROUPMETRICS = (FIRST + 155),
                GETGROUPMETRICS = (FIRST + 156),
                ENABLEGROUPVIEW = (FIRST + 157),
                SORTGROUPS = (FIRST + 158),
                INSERTGROUPSORTED = (FIRST + 159),
                REMOVEALLGROUPS = (FIRST + 160),
                HASGROUP = (FIRST + 161),
                GETGROUPSTATE = (FIRST + 92),
                GETFOCUSEDGROUP = (FIRST + 93),
                SETTILEVIEWINFO = (FIRST + 162),
                GETTILEVIEWINFO = (FIRST + 163),
                SETTILEINFO = (FIRST + 164),
                GETTILEINFO = (FIRST + 165),
                SETINSERTMARK = (FIRST + 166),
                GETINSERTMARK = (FIRST + 167),
                INSERTMARKHITTEST = (FIRST + 168),
                GETINSERTMARKRECT = (FIRST + 169),
                SETINSERTMARKCOLOR = (FIRST + 170),
                GETINSERTMARKCOLOR = (FIRST + 171),
                GETSELECTEDCOLUMN = (FIRST + 174),
                ISGROUPVIEWENABLED = (FIRST + 175),
                GETOUTLINECOLOR = (FIRST + 176),
                SETOUTLINECOLOR = (FIRST + 177),
                CANCELEDITLABEL = (FIRST + 179),
                MAPINDEXTOID = (FIRST + 180),
                MAPIDTOINDEX = (FIRST + 181),
                ISITEMVISIBLE = (FIRST + 182),
                GETACCVERSION = (FIRST + 193),
                GETEMPTYTEXT = (FIRST + 204),
                GETFOOTERRECT = (FIRST + 205),
                GETFOOTERINFO = (FIRST + 206),
                GETFOOTERITEMRECT = (FIRST + 207),
                GETFOOTERITEM = (FIRST + 208),
                GETITEMINDEXRECT = (FIRST + 209),
                SETITEMINDEXSTATE = (FIRST + 210),
                GETNEXTITEMINDEX = (FIRST + 211),
                SETPRESERVEALPHA = (FIRST + 212),
                SETBKIMAGE = SETBKIMAGEW,
                GETBKIMAGE = GETBKIMAGEW,
                //SETBKIMAGE             = SETBKIMAGEA,
                //GETBKIMAGE             = GETBKIMAGEA,
            }

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
                string windowTitle);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref IntPtr lParam);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct POINT
            {
                public long x;
                public long y;
            }

            public static void Test()
            {
                //IntPtr handle = FindWindow("Progman", null);
                //handle = FindWindowEx(handle, IntPtr.Zero, "SHELLDLL_DefView", null);
                var sb = new StringBuilder(256);

                var handle = IntPtr.Zero;

                W32.EnumChildWindows(W32.GetDesktopWindow(), (hwnd, param) =>
                {
                    if (W32.GetClassName(hwnd, sb, 256) > 0)
                    {
                        if (sb.ToString() == "SysListView32")
                        {
                            handle = hwnd;
                        }
                    }

                    sb.Clear();
                    return true;
                }, IntPtr.Zero);

                //Get the Number of Icons
                int iconCount = SendMessage(handle, (int)LVM.GETITEMCOUNT, 0, IntPtr.Zero);

                Console.WriteLine("Number of Icons on Desktop: " + iconCount);
            }
        }
        */
    }

    internal class ParticleManager
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

    internal class ParticleSquare : Particle
    {
        public ParticleSquare(float x, float y, float mx, float my, int maxAge, float size) : base(x, y, mx, my, maxAge, size)
        {
            var mult = Math.Max(Math.Min(32 / size, 4), 1);

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

            //GL.PushAttrib(AttribMask.LineBit);
            GL.PushMatrix();
            //GL.LineWidth(deltaSize / StartSize);

            GL.Translate(deltaX, deltaY, 0);
            GL.Rotate(deltaAngle, 0, 0, 1);
            GL.Scale(deltaSize, deltaSize, 0);

            GL.Begin(PrimitiveType.Polygon);

            GL.Color4(c.X, c.Y, c.Z, deltaAlpha * 0.15);
            GL.Vertex2(-0.5, -0.5);
            GL.Vertex2(-0.5, 0.5);
            GL.Vertex2(0.5, 0.5);
            GL.Vertex2(0.5, -0.5);

            GL.End();

            GL.Begin(PrimitiveType.LineLoop);

            GL.Color4(c.X, c.Y, c.Z, deltaAlpha);
            GL.Vertex2(-0.5, -0.5);
            GL.Vertex2(-0.5, 0.5);
            GL.Vertex2(0.5, 0.5);
            GL.Vertex2(0.5, -0.5);

            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            VertexUtil.PutCircle(-0.5f, -0.5f, .25f);
            GL.End();
            GL.Begin(PrimitiveType.Polygon);
            VertexUtil.PutCircle(-0.5f, 0.5f, .25f);
            GL.End();
            GL.Begin(PrimitiveType.Polygon);
            VertexUtil.PutCircle(0.5f, 0.5f, .25f);
            GL.End();
            GL.Begin(PrimitiveType.Polygon);
            VertexUtil.PutCircle(0.5f, -0.5f, .25f);
            GL.End();

            GL.PopMatrix();
           // GL.PopAttrib();
        }
    }

    internal class ParticleTriangle : Particle
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

            var x = 0.43301270189222f;

           // GL.PushAttrib(AttribMask.LineBit);
            GL.PushMatrix();
            //GL.LineWidth(deltaSize / StartSize);

            GL.Translate(deltaX, deltaY, 0);
            GL.Rotate(deltaAngle, 0, 0, 1);
            GL.Scale(deltaSize, deltaSize, 0);

            GL.Begin(PrimitiveType.Polygon);

            GL.Color4(c.X, c.Y, c.Z, deltaAlpha * 0.15);
            GL.Vertex2(0, -x);
            GL.Vertex2(-0.5, x);
            GL.Vertex2(0.5, x);

            GL.End();

            GL.Begin(PrimitiveType.LineLoop);

            GL.Color4(c.X, c.Y, c.Z, deltaAlpha);
            GL.Vertex2(0, -x);
            GL.Vertex2(-0.5, x);
            GL.Vertex2(0.5, x);

            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            VertexUtil.PutCircle(0, -x, .25f);
            GL.End();
            GL.Begin(PrimitiveType.Polygon);
            VertexUtil.PutCircle(-0.5f, x, .25f);
            GL.End();
            GL.Begin(PrimitiveType.Polygon);
            VertexUtil.PutCircle(0.5f, x, .25f);

            GL.End();
            GL.PopMatrix();
            //GL.PopAttrib();
        }
    }

    internal class ParticleCircle : Particle
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
            
           // GL.PushAttrib(AttribMask.LineBit);
            GL.PushMatrix();
            //GL.LineWidth(deltaSize / StartSize);

            GL.Translate(deltaX, deltaY, 0);
            GL.Rotate(deltaAngle, 0, 0, 1);
            GL.Scale(deltaSize, deltaSize, 0);
            
            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(c.X, c.Y, c.Z, deltaAlpha * 0.15);
            VertexUtil.PutCircle(0, 0, 1, 20);
            GL.End();
          
           
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(c.X, c.Y, c.Z, deltaAlpha);
            VertexUtil.PutCircle(0, 0, 1, 20);
            GL.End();

            GL.PopMatrix();
            //GL.PopAttrib();
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

        public float Acceleration = 1;

        protected int _direction;

        public bool IsDead;

        protected Particle(float x, float y, float mx, float my, int maxAge, float size)
        {
            PrevX = X = x;
            PrevY = Y = y;

            Mx = mx;
            My = my;

            MaxAge = maxAge;
            StartSize = size;

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

            if (Age <= 2)
            {
                Size = Age / 2f * StartSize;
            }

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

            var mult = (float)Math.Min(Math.Sqrt(Mx * Mx + My * My), 4) * 3 * _direction;

            Angle += mult;

            X += Mx;
            Y += My;

            Mx *= 0.9875f * Acceleration;
            My *= 0.9875f * Acceleration;
        }

        public virtual void Render(float deltaTime)
        {
        }
    }

    internal static class Hue
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

    public static class VertexUtil
    {
        public static void PutCircle(float centerX = 0, float centerY = 0, float size = 1, int points = 10)
        {
            for (int i = points - 1; i >= 0; i--)
            {
                var a = i / (float)points;

                var x = Math.Cos(a * MathHelper.TwoPi) / 2 * size;
                var y = Math.Sin(a * MathHelper.TwoPi) / 2 * size;

                GL.Vertex2(x + centerX, y + centerY);
            }
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

            //IntPtr dc = W32.GetDCEx(workerw, IntPtr.Zero, (W32.DeviceContextValues)0x403);
            //W32.ReleaseDC(workerw, dc);

            if (W32.GetParent(form.WindowInfo.Handle) != workerw) //if the window is not a child of WorkerW already
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

        public static void ReloadWallpaper()
        {
            try
            {
                var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                string tempPath = $"{Path.GetTempFileName()}.jpg";

                File.Copy($@"{appdata}\Microsoft\Windows\Themes\TranscodedWallpaper", tempPath);

                W32.SystemParametersInfo(20,
                    0,
                    tempPath,
                    3);
            }
            catch { }
        }

        public static void RefreshExplorer()
        {
            W32.SendMessageTimeout(new IntPtr(0xffff), 0x1a, IntPtr.Zero, IntPtr.Zero, W32.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 100, out var result);
        }

        public static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            var placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(
            IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        public enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }
    }
}