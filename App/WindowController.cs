using System.Diagnostics;

namespace GenshinJPTextSpeaker
{
    public class WindowController
    {
        #region Static Fields
        public static WindowController CreateInstance(IntPtr windowHandle)
        {
            return new WindowController(windowHandle);
        }
        public static WindowController FindWindow(string windowTitle = null)
        {
            return CreateInstance(Win32Api.FindWindow(windowTitle));
        }

        public static WindowController FindWindow(string windowTitle = null, string className = null)
        {
            return CreateInstance(Win32Api.FindWindow(windowTitle, className));
        }

        public static WindowController FindWindow(IntPtr windowHandle, string windowTitle = null, string className = null)
        {
            return CreateInstance(Win32Api.FindWindow(windowHandle, windowTitle, className));
        }

        public static WindowController FindWindow(WindowController windowController, string windowTitle = null, string className = null)
        {
            return CreateInstance(Win32Api.FindWindow(windowController.Handle, windowTitle, className));
        }

        public static WindowController FindWindowTitleStartsWith(string text)
        {
            return CreateInstance(IntPtr.Zero)
                    .ChildWindowCollection
                    .FirstOrDefault(x => x.Title.IgnoreCaseStartsWith(text));
        }

        public static WindowController FindWindowTitleEndsWith(string text)
        {
            return CreateInstance(IntPtr.Zero)
                    .ChildWindowCollection
                    .FirstOrDefault(x => x.Title.IgnoreCaseEndsWith(text));
        }

        public static WindowController FindWindowTitleContains(string text)
        {
            return CreateInstance(IntPtr.Zero)
                    .ChildWindowCollection
                    .FirstOrDefault(x => x.Title.IgnoreCaseContains(text));
        }
        #endregion

        public IntPtr Handle { get; }

        public WindowController Parent => CreateInstance(Win32Api.GetParent(Handle));

        public string Title
        {
            get => Win32Api.GetWindowTitle(Handle);
            set => Win32Api.SetWindowTitle(Handle, value);
        }

        public string ClassName => Win32Api.GetClassName(Handle);

        public Bounds Bounds
        {
            get => Win32Api.GetWindowBounds(Handle);
            set => Win32Api.SetWindowBounds(Handle, value, WindowPositionFlags.ShowWindow);
        }

        public Bounds ClientBounds => Win32Api.GetWindowClientBounds(Handle);

        public string Text
        {
            get => Win32Api.GetText(Handle);
            set => Win32Api.SetText(Handle, value);
        }

        public bool AlwaysOnTop
        {
            get => Win32Api.IsAlwaysOnTopEnabled(Handle);
            set => Win32Api.SetAlwaysOnTop(Handle, value);
        }

        public WindowStates State
        {
            get => Win32Api.GetWindowState(Handle);
            set => Win32Api.SetWindowState(Handle, value);
        }

        public string ProcessName => Win32Api.GetProcessNameByHandle(Handle);

        public int ProcessId => Win32Api.GetProcessId(Handle);

        public bool IsMaximized => Win32Api.IsZoomed(Handle);

        public bool IsMinimized => Win32Api.IsIconic(Handle);

        public bool IsWindow => Win32Api.IsWindow(Handle);

        public bool Exists => Win32Api.IsWindow(Handle);

        public bool IsWindowVisible => Win32Api.IsWindowVisible(Handle);

        public List<WindowController> ChildWindows
        {
            get
            {
                return Win32Api.EnumChildWindows(Handle)
                    .Select(x => CreateInstance(x))
                    .ToList();
            }
        }

        public IEnumerable<WindowController> ChildWindowCollection
        {
            get
            {
                return Win32Api.EnumChildWindows(Handle).Select(x => CreateInstance(x));
            }
        }

        readonly long _defaultStyle, _defaultExStyle;

        public WindowController(IntPtr handle)
        {
            Handle = handle;
            _defaultStyle = Win32Api.GetWindowLong(Handle, GWL.Style);
            _defaultExStyle = Win32Api.GetWindowLong(Handle, GWL.ExStyle);
        }

        public WindowController FindChild(string windowTitle = null, string className = null)
        {
            var hWnd = Win32Api.FindWindow(Handle, windowTitle, className);
            return hWnd == IntPtr.Zero ? null : CreateInstance(hWnd);
        }

        public void SetBounds(Bounds bounds, WindowInsertAfter windowInsertAfter, WindowPositionFlags windowPositionFlags = WindowPositionFlags.ShowWindow)
        {
            Win32Api.SetWindowBounds(Handle, bounds, windowInsertAfter, windowPositionFlags);
        }

        public void SetBounds(int left, int top, int width, int height, WindowInsertAfter windowInsertAfter, WindowPositionFlags windowPositionFlags)
        {
            var bounds = new Bounds(left, top, width, height);
            Win32Api.SetWindowBounds(Handle, bounds, windowInsertAfter, windowPositionFlags);
        }

        public void MoveTo(int left, int top)
        {
            var (_, _, width, height) = Bounds.ToTuple();
            Bounds = new Bounds(left, top, width, height);
        }

        public void Resize(int width, int height)
        {
            var (left, top, _, _) = Bounds.ToTuple();
            Bounds = new Bounds(left, top, width, height);
        }

        public void KillProcess()
        {
            Process.GetProcessById(ProcessId)?.Kill();
        }

        public void Activate()
        {
            Win32Api.Activate(Handle);
        }

        public void Close()
        {
            Win32Api.CloseWindow(Handle);
        }

        public void Maximize()
        {
            Win32Api.SetWindowState(Handle, WindowStates.ShowMaximized);
        }

        public void Minimize()
        {
            Win32Api.SetWindowState(Handle, WindowStates.Minimized);
        }

        public void Restore()
        {
            Win32Api.SetWindowState(Handle, WindowStates.Restore);
        }
    }
}
