using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GenshinJPTextSpeaker
{
    public class Win32Api
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #region Send / Post Message
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int SendMessage(IntPtr hWnd, uint message, uint wParam, uint lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int SendMessage(IntPtr hwnd, int msg, int wParam, StringBuilder stringBuilder);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int SendMessage(IntPtr hwnd, int msg, int wParam, string text);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, uint lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int PostMessage(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam);
        #endregion

        [DllImport("kernel32.dll")] public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr WindowFromPoint(Point p);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr WindowFromPhysicalPoint(Point r);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildafter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)] public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool EnumChildWindows(IntPtr hWnd, EnumWindowsProc proc, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool IsIconic(IntPtr handle);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool IsZoomed(IntPtr handle);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool IsWindowVisible(IntPtr handle);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool ShowWindowAsync(IntPtr handle, WindowStates state);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, WindowPositionFlags flags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern long SetWindowLong(IntPtr hWnd, GWL nIndex, long dwLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern long GetWindowLong(IntPtr handle, GWL nIndex);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool DestroyWindow(IntPtr handle);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern void GetWindowThreadProcessId(IntPtr hWnd, out int id);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetClassName(IntPtr handle, StringBuilder sb, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool SetWindowText(IntPtr hWnd, string title);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetWindowText(IntPtr handle, StringBuilder sb, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int GetWindowTextLength(IntPtr handle);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool GetWindowRect(IntPtr handle, out Rect r);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool GetClientRect(IntPtr handle, out Rect r);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool ClientToScreen(IntPtr handle, out Point r);
        [DllImport("dwmapi.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out Rect pvAttribute, int cbAttribute);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern uint GetGUIThreadInfo(uint dwthreadid, ref GuiThreadInformation lpguithreadinfo);
        [DllImport("imm32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool GetAsyncKeyState(Keys KeyCode);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] public static extern bool GetCursorPos(out Point point);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern bool CloseHandle(IntPtr handle);
        [DllImport("psapi.dll", CharSet = CharSet.Ansi, SetLastError = true)] private static extern uint GetModuleBaseName(IntPtr hWnd, IntPtr hModule, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder lpBaseName, uint nSize);


        [DllImport("user32.dll")]
        public static extern uint GetDpiForWindow(IntPtr hwnd);

        public static double GetDpiFrom(IntPtr hWnd)
        {
            return GetDpiForWindow(hWnd) / 96.0;
        }


        static Process[] processes = new Process[0];
        static int recursiveCount = 0;

        public static string GetProcessNameByHandle(IntPtr windowHandle)
        {
            var processName = "";

            GetWindowThreadProcessId(windowHandle, out var id);
            var process = processes.FirstOrDefault(x => x.Id == id);
            if (process != null)
            {
                processName = process.ProcessName;
                recursiveCount = 0;
            }
            else
            {
                processes = Process.GetProcesses();
                if (++recursiveCount > 100)
                {
                    recursiveCount = 0;
                    return "";
                }
                return GetProcessNameByHandle(windowHandle);
            }

            if (processName == "ApplicationFrameHost")
            {
                var window = WindowController
                    .CreateInstance(windowHandle)
                    .ChildWindows.Find(x => x.ClassName == "Windows.UI.Core.CoreWindow");

                return window?.ProcessName ?? processName;
            }
            else
            {
                return processName;
            }
        }

        public static void ShowOrHideConsole(bool willShow)
        {
            var newState = willShow ? WindowStates.Show : WindowStates.Hide;
            var hWnd = GetConsoleWindow();
            var window = WindowController.CreateInstance(hWnd);
            var consoleWindow = window.ClassName == "PseudoConsoleWindow" ? window.Parent : window;
            consoleWindow.State = newState;
        }

        public static void SetTopmost(IntPtr handle, bool topmost)
        {
            var insertAfter = topmost ? WindowInsertAfter.Topmost : WindowInsertAfter.NoTopmost;
            SetWindowPos(handle, insertAfter.Handle, 0, 0, 0, 0, WindowPositionFlags.NoSize | WindowPositionFlags.NoMove);
        }

        public static bool IsImeEnabled()
        {
            var gti = new GuiThreadInformation();
            gti.cbSize = Marshal.SizeOf(gti);
            var guiInfo = GetGUIThreadInfo(0, ref gti);
            var immWindow = ImmGetDefaultIMEWnd(gti.hwndFocus);
            var imeEnabled = SendMessage(immWindow, WindowMessage.WM_IME_CONTROL, IMC.IMC_GETOPENSTATUS, 0);
            var imeConverionMode = SendMessage(immWindow, WindowMessage.WM_IME_CONTROL, IMC.IMC_GETCONVERSIONMODE, 0);
            return !(imeEnabled == 0 && imeConverionMode == 0);
        }

        public static void Activate(IntPtr handle)
        {
            if (IsIconic(handle))
            {
                ShowWindowAsync(handle, WindowStates.Restore);
            }
            else
            {
                SetForegroundWindow(handle);
            }
        }

        public static List<IntPtr> EnumChildWindows(IntPtr windowHandle)
        {
            var windows = new List<IntPtr>();
            EnumChildWindows(windowHandle, proc, IntPtr.Zero);
            bool proc(IntPtr hWndChild, IntPtr lParam)
            {
                if (hWndChild != IntPtr.Zero)
                    windows.Add(hWndChild);
                return true;
            }
            return windows;
        }

        #region FindWindow
        public static IntPtr FindWindow(string windowTitle = null)
        {
            return FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, windowTitle);
        }

        public static IntPtr FindWindow(string windowTitle = null, string className = null)
        {
            return FindWindowEx(IntPtr.Zero, IntPtr.Zero, className, windowTitle);
        }

        public static IntPtr FindWindow(IntPtr windowHandle, string windowTitle = null, string className = null)
        {
            return FindWindowEx(windowHandle, IntPtr.Zero, className, windowTitle);
        }
        #endregion

        #region Bounds
        public static Bounds GetWindowBounds(IntPtr handle)
        {
            var size = Marshal.SizeOf(typeof(Win32Api.Rect));
            var attr = DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS;
            DwmGetWindowAttribute(handle, attr, out var r, size);
            return new Bounds(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

        public static void SetWindowBounds(IntPtr handle, Bounds bounds, WindowPositionFlags flags)
        {
            var insertAfter = IsAlwaysOnTopEnabled(handle) ? WindowInsertAfter.Topmost : WindowInsertAfter.NoTopmost;
            var (left, top, width, height) = bounds.ToTuple();
            SetWindowPos(handle, insertAfter.Handle, left, top, width, height, WindowPositionFlags.ShowWindow);
        }

        public static void SetWindowBounds(IntPtr handle, Bounds bounds, WindowInsertAfter windowInsertAfter, WindowPositionFlags windowPositionFlags)
        {
            var (left, top, width, height) = bounds.ToTuple();
            SetWindowPos(handle, windowInsertAfter.Handle, left, top, width, height, windowPositionFlags);
        }

        public static Bounds GetWindowClientBounds(IntPtr handle)
        {
            GetClientRect(handle, out var r);
            ClientToScreen(handle, out var p);
            return new Bounds(r.Left + p.X, r.Top + p.Y, r.Right - r.Left, r.Bottom - r.Top);
        }

        #endregion

        #region GetClassName
        public static string GetClassName(IntPtr handle)
        {
            var sb = new StringBuilder(short.MaxValue);
            GetClassName(handle, sb, sb.Capacity);
            return sb.ToString();
        }
        #endregion

        #region Title
        public static string GetWindowTitle(IntPtr handle)
        {
            var sb = new StringBuilder(short.MaxValue);
            GetWindowText(handle, sb, sb.Capacity);
            return sb.ToString();
        }

        public static void SetWindowTitle(IntPtr handle, string text)
        {
            SetWindowText(handle, text);
        }
        #endregion

        #region Text
        public static string GetText(IntPtr handle)
        {
            var length = SendMessage(handle, WindowMessage.WM_GETTEXTLENGTH, 0, 0);
            var sb = new StringBuilder(length + 1);
            SendMessage(handle, WindowMessage.WM_GETTEXT, sb.Capacity, sb);
            return sb.ToString();
        }

        public static void SetText(IntPtr handle, string text)
        {
            SendMessage(handle, WindowMessage.WM_SETTEXT, 0, text);
        }
        #endregion

        #region AlwaysOnTop
        public static bool IsAlwaysOnTopEnabled(IntPtr handle)
        {
            var style = GetWindowLong(handle, GWL.ExStyle);
            var topmost = (long)ExWindowStyles.WS_EX_TOPMOST;
            return (style & topmost) == topmost;
        }

        public static void SetAlwaysOnTop(IntPtr handle, bool alwaysOnTop)
        {
            var windowInsertAfter = alwaysOnTop ? WindowInsertAfter.Topmost : WindowInsertAfter.NoTopmost;
            SetWindowPos(handle, windowInsertAfter.Handle, 0, 0, 0, 0, WindowPositionFlags.NoSizeNoMove);
        }
        #endregion

        #region WindowState
        public static WindowStates GetWindowState(IntPtr handle)
        {
            var placement = WINDOWPLACEMENT.Default;
            GetWindowPlacement(handle, ref placement);
            return placement.ShowCmd;
        }

        public static void SetWindowState(IntPtr handle, WindowStates state)
        {
            ShowWindowAsync(handle, state);
        }
        #endregion

        #region GetProcessId
        public static int GetProcessId(IntPtr handle)
        {
            GetWindowThreadProcessId(handle, out var id);
            return id;
        }
        #endregion

        #region Close Window
        public static void CloseWindow(IntPtr handle)
        {
            SendMessage(handle, WindowMessage.WM_CLOSE, 0, 0);
        }
        #endregion

        #region
        #endregion

        #region Cursor
        [DllImport("user32.dll")] public static extern bool SetSystemCursor(IntPtr hcur, SystemCursors cursor);
        [DllImport("user32.dll")] public static extern IntPtr CopyIcon(IntPtr pcur);
        [DllImport("user32.dll")] public static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
        [DllImport("user32.dll")] public static extern IntPtr LoadCursorFromFile(string path);

        public const uint SPI_SETCURSORS = 0x0057;
        #endregion

        #region const

        public class WindowMessage
        {
            public const int WM_SIZE = 0x0005;
            public const int WM_SIZING = 0x0214;
            public const int WM_ENTERSIZEMOVE = 0x0231;
            public const int WM_EXITSIZEMOVE = 0x0232;
            public const int WM_NCLBUTTONDBLCLK = 0xA3;

            public const uint WM_KEYDOWN = 0x0100;
            public const uint WM_KEYUP = 0x0101;
            public const uint WM_LBUTTONDOWN = 0x0201;
            public const uint WM_LBUTTONUP = 0x0202;
            public const uint WM_RBUTTONDOWN = 0x0204;
            public const uint WM_RBUTTONUP = 0x0205;
            public const uint WM_MBUTTONDOWN = 0x0207;
            public const uint WM_MBUTTONUP = 0x0208;
            public const uint WM_XBUTTONDOWN = 0x020B;
            public const uint WM_XBUTTONUP = 0x020C;
            public const uint WM_IME_CONTROL = 0x283;
            public const uint WM_POWERBROADCAST = 0x218;
            public const uint WM_DEVICECHANGE = 0x219; // 537
            public const int WM_SETTEXT = 0x000C;
            public const int WM_GETTEXT = 0x000D;
            public const int WM_GETTEXTLENGTH = 0x000E;
            public const uint WM_CLOSE = 0x0010;
            public const int BM_CLICK = 0x00F5;
            public const int WM_SYSCOMMAND = 0x112;
            public const int SC_MOVE = 0xF010;
        }

        public class IMC
        {
            public const uint IMC_GETCONVERSIONMODE = 1;
            public const uint IMC_SETCONVERSIONMODE = 2;
            public const uint IMC_GETOPENSTATUS = 5;
            public const uint IMC_SETOPENSTATUS = 6;
        }
        #endregion

        #region マウス加速


        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        public static extern bool SystemParametersInfoGet(uint action, uint param, IntPtr vparam, SPIF fWinIni);
        public const uint SPI_GETMOUSE = 0x0003;

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        public static extern bool SystemParametersInfoSet(uint action, uint param, IntPtr vparam, SPIF fWinIni);
        public const uint SPI_SETMOUSE = 0x0004;

        public static bool IsMouseAccelerationEnabled(out int[] mouseParams)
        {
            mouseParams = new int[3];
            SystemParametersInfoGet(SPI_GETMOUSE, 0, GCHandle.Alloc(mouseParams, GCHandleType.Pinned).AddrOfPinnedObject(), 0);
            return mouseParams[2] == 1;
        }

        public static void SetMouseAccelerationEnabled(bool isEnabled)
        {
            var isAccelerationEnabled = IsMouseAccelerationEnabled(out var mouseParams);
            if (isAccelerationEnabled == isEnabled)
            {
                return;
            }

            mouseParams[2] = isEnabled ? 1 : 0;

            SystemParametersInfoSet(SPI_SETMOUSE, 0, GCHandle.Alloc(mouseParams, GCHandleType.Pinned).AddrOfPinnedObject(), SPIF.SPIF_SENDCHANGE);
        }

        [Flags]
        public enum SPIF
        {
            None = 0x00,
            SPIF_UPDATEINIFILE = 0x01,
            SPIF_SENDCHANGE = 0x02,
            SPIF_SENDWININICHANGE = 0x02
        }

        #endregion

        #region
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GuiThreadInformation
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public System.Drawing.Rectangle rcCaret;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int Length;

            public int Flags;

            public WindowStates ShowCmd;

            public Point MinPosition;

            public Point MaxPosition;

            public Rect NormalPosition;

            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.Length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }
        #endregion

        #region Enums

        public enum SystemCursors
        {
            AppStarting = 32650,
            Normal = 32512,
            Cross = 32515,
            Hand = 32649,
            Help = 32651,
            Ibeam = 32513,
            No = 32648,
            SizeAll = 32646,
            SizeNESW = 32643,
            SizeNS = 32645,
            SizeS = 32645,
            SizeNWSE = 32642,
            SizeWE = 32644,
            Up = 32516,
            Wait = 32514,
        }

        [Flags]
        public enum DwmWindowAttribute : int
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        #endregion

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, MouseHookCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint msg, ref KeyboardLowLevelHookStruct kbdllhookstruct);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint msg, ref MouseLowLevelHookStruct mouse);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern int MapVirtualKey(int KeyCode);

        /// <summary>
        /// ウィンドウメッセージ、マウスの挙動
        /// </summary>
        public const int WM_MOUSE_MOVE = 512;
        public const int WM_LEFT_DOWN = 0x201;
        public const int WM_LEFT_UP = 0x202;
        public const int WM_RIGHT_DOWN = 0x204;
        public const int WM_RIGHT_UP = 0x205;
        public const int WM_MIDDLE_DOWN = 0x207;
        public const int WM_MIDDLE_UP = 0x208;
        public const int WM_XBUTTON_DOWN = 0x20B;
        public const int WM_XBUTTON_UP = 0x20C;
        public const int WM_V_WHEEL = 522;
        public const int WM_H_WHEEL = 526;

        /// <summary>
        /// ウィンドウメッセージ、キーの挙動
        /// </summary>
        public const uint WM_KEY_DOWN = 0x100;
        public const uint WM_KEY_UP = 0x101;
        public const uint WM_SYS_KEY_DOWN = 0x104;
        public const uint WM_SYS_KEY_UP = 0x105;

        public const int EXTENDED_KEY = 0x0;
        public const int INJECTED_KEY = 0x10;
        public const int ALTDOWN_KEY = 0x1;

        public const int ToggleCapsLock = 240;
        public const int ToggleKanaHira1 = 242;
        public const int ToggleKanaHira2 = 241;
        public const int ToggleKanaHira3 = 245;

        public delegate IntPtr KeyboardHookCallback(int nCode, uint wParam, ref KeyboardLowLevelHookStruct lParam);

        public delegate IntPtr MouseHookCallback(int nCode, uint wParam, ref MouseLowLevelHookStruct lParam);
    }

    public struct KeyboardLowLevelHookStruct
    {
        public int VirtualKeyCode;
        public int ScanCode;
        public int Flag;
        public int Time;
        public IntPtr Info;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseLowLevelHookStruct
    {
        public Point Point;
        public int MouseData;
        public int Flags;
        public int Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    public class Bounds
    {
        public Bounds(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public Bounds(double left, double top, double width, double height)
        {
            Left = (int)left;
            Top = (int)top;
            Width = (int)width;
            Height = (int)height;
        }

        public Bounds()
        {
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Bottom => Top + Height;

        public int Right => Left + Width;

        public (int X, int Y) Center => (CenterX, CenterY);

        public (int width, int height) Size => (Width, Height);

        public (int left, int top) Point => (Width, Height);

        public int CenterX => Left + Width / 2;

        public int CenterY => Top + Height / 2;

        public (int left, int top, int width, int height) ToTuple() => (Left, Top, Width, Height);

        public (double left, double top, double width, double height) ToTupleAsDouble() => (Left, Top, Width, Height);

        public Bounds Clone()
        {
            return new Bounds(Left, Top, Width, Height);
        }
    }

    public enum WindowStates : int
    {
        Hide = 0,
        ShowNormal = 1,
        ShowMinimized = 2,
        ShowMaximized = 3,
        ShowNoActivate = 4,
        Show = 5,
        Minimized = 6,
        ShowMinNoActive = 7,
        ShowNa = 8,
        Restore = 9,
        ShowDefault = 10,
        ForceMinimize = 11,
    }

    public class WindowInsertAfter
    {
        public static WindowInsertAfter NoTopmost => new WindowInsertAfter(-2);
        public static WindowInsertAfter Topmost => new WindowInsertAfter(-1);
        public static WindowInsertAfter Top => new WindowInsertAfter(0);
        public static WindowInsertAfter Bottom => new WindowInsertAfter(1);

        public IntPtr Handle { get; }

        WindowInsertAfter(int value)
        {
            this.Handle = new IntPtr(value);
        }
    }

    public enum WindowPositionFlags : uint
    {

        NoSize = 0x0001,
        NoMove = 0x0002,
        NoZOrder = 0x0004,
        NoRedraw = 0x0008,
        NoActive = 0x0010,
        DrawFrame = 0x0020,
        FrameChanged = 0x0020,
        ShowWindow = 0x0040,
        HideWindow = 0x0080,
        NoSendChanging = 0x0400,
        AsyncWindowPosition = 0x4000,
        DefererErase = 0x2000,
        NoSizeNoMove = NoSize | NoMove
    }

    public enum GWL : int
    {
        WndProc = -4,
        HInstance = -6,
        Id = -12,
        Style = -16,
        ExStyle = -20,
        UserData = -21,
    }

    public enum WindowStyles : long
    {
        Overlapped = 0x00000000,
        Popup = 0x80000000,
        Child = 0x40000000,
        ChildWindow = 0x40000000,
        Minimize = 0x20000000,
        Visible = 0x10000000,
        Disabled = 0x08000000,
        ClipSiblings = 0x04000000,
        ClipChildren = 0x02000000,
        Caption = 0x00C00000,
        Border = 0x00800000,
        DialogFrame = 0x00400000,
        VerticalScroll = 0x00200000,
        HorizontalScroll = 0x00100000,
        SystemMenu = 0x00080000,
        ThickFrame = 0x00040000,
        SizeBox = 0x00040000,
        Group = 0x00020000,
        TabStop = 0x00010000,
        MinimizeBox = 0x00020000,
        MaximizeBox = 0x00010000,
        OverlappedWindow = Overlapped | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox,
        PopupWindow = Popup | Border | SystemMenu
    }

    public enum ExWindowStyles
    {
        WS_EX_DLGMODALFRAME = 0x00000001,
        WS_EX_NOPARENTNOTIFY = 0x00000004,
        WS_EX_TOPMOST = 0x00000008,
        WS_EX_ACCEPTFILES = 0x00000010,
        WS_EX_TRANSPARENT = 0x00000020,
        WS_EX_MDICHILD = 0x00000040,
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_WINDOWEDGE = 0x00000100,
        WS_EX_CLIENTEDGE = 0x00000200,
        WS_EX_CONTEXTHELP = 0x00000400,
        WS_EX_RIGHT = 0x00001000,
        WS_EX_LEFT = 0x00000000,
        WS_EX_RTLREADING = 0x00002000,
        WS_EX_LTRREADING = 0x00000000,
        WS_EX_LEFTSCROLLBAR = 0x00004000,
        WS_EX_LAYERED = 0x00080000,
        WS_EX_RIGHTSCROLLBAR = 0x00000000,
        WS_EX_CONTROLPARENT = 0x00010000,
        WS_EX_STATICEDGE = 0x00020000,
        WS_EX_APPWINDOW = 0x00040000,
        WS_EX_OVERLAPPEDWINDOW = 0x00000300,
        WS_EX_PALETTEWINDOW = 0x00000188,
        WS_EX_COMPOSITED = 0x02000000,
        WS_EX_LAYOUTRTL = 0x00400000,
        WS_EX_NOACTIVATE = 0x08000000,
        WS_EX_NOINHERITLAYOUT = 0x00100000,
        WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
    }
}
