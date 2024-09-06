using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using static GenshinJPTextSpeaker.Win32Api;

namespace GenshinJPTextSpeaker
{
    public class HookEventArgs
    {
        public Keys Key { get; set; }

        public bool State { get; set; }

        public bool IsInjected { get; set; }

        public bool WillCancel { get; set; }

        public bool PreviousState { get; set; }

        public bool HasStateChanged => State != PreviousState;
    }

    public class InputStateManager
    {
        public static ConcurrentDictionary<Keys, bool> Status { get; set; } = new ConcurrentDictionary<Keys, bool>();

        static InputStateManager()
        {
            var keys = Enum.GetValues(typeof(Keys)).OfType<Keys>().Distinct().ToDictionary(key => key, key => false);
            Status = new ConcurrentDictionary<Keys, bool>(keys);
        }

        public static bool GetState(Keys key)
        {
            if (!Status.ContainsKey(key))
            {
                return false;
            }

            return Status[key];
        }

        public static void SetState(Keys key, bool newStatus)
        {
            if (Status.ContainsKey(key))
            {
                Status[key] = newStatus;
            }
        }
    }

    public class KeyboardHooker
    {
        public event EventHandler<HookEventArgs> Hooked;

        Win32Api.KeyboardHookCallback _hookProcedure;
        IntPtr _hookHandle;

        public KeyboardHooker() : base()
        {
            _hookProcedure = KeyboardHookProcedure;
        }

        public void Start()
        {
            var handle = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]);
            var WH_KEYBOARD_LL = 13;
            _hookHandle = Win32Api.SetWindowsHookEx(WH_KEYBOARD_LL, _hookProcedure, handle, 0);
        }

        public void Stop()
        {
            Win32Api.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }

        private IntPtr KeyboardHookProcedure(int nCode, uint wParam, ref KeyboardLowLevelHookStruct lParam)
        {
            if (nCode >= 0)
            {
                var isInjected = (lParam.Flag & Win32Api.INJECTED_KEY) != 0;
                var state = wParam switch
                {
                    Win32Api.WM_KEY_DOWN or Win32Api.WM_SYS_KEY_DOWN => true,
                    Win32Api.WM_KEY_UP or Win32Api.WM_SYS_KEY_UP => false,
                    _ => false
                };
                var key = (Keys)lParam.VirtualKeyCode;
                var eventArgs = new HookEventArgs 
                {
                    Key = key,
                    State = state,
                    IsInjected = isInjected, 
                    PreviousState = InputStateManager.GetState(key) 
                };
                InputStateManager.SetState(key, state);
                Hooked?.Invoke(this, eventArgs);
                if (eventArgs.WillCancel)
                {
                    return (IntPtr)1;
                }
            }

            // 次のフックプロシージャへ
            return Win32Api.CallNextHookEx(_hookHandle, nCode, wParam, ref lParam);
        }
    }

    public class MouseHooker
    {
        public event EventHandler<HookEventArgs> Hooked;

        MouseHookCallback _hookProcedure;
        IntPtr _hookHandle;

        public MouseHooker()
        {
            _hookProcedure = MouseHookProcedure;
        }

        public void Start()
        {
            var handle = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]);
            var WH_MOUSE_LL = 14;
            _hookHandle = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, _hookProcedure, handle, 0);
        }

        public void Stop()
        {
            Win32Api.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }

        private IntPtr MouseHookProcedure(int nCode, uint wParam, ref MouseLowLevelHookStruct lParam)
        {
            try
            {
                if (wParam == Win32Api.WM_MOUSE_MOVE
                    || wParam == Win32Api.WM_V_WHEEL 
                    || wParam == Win32Api.WM_H_WHEEL)
                {
                    return Win32Api.CallNextHookEx(_hookHandle, nCode, wParam, ref lParam);
                }

                if (nCode >= 0)
                {
                    var isInjected = (lParam.Flags & 0x1) == 1;
                    var eventArgs = CreateMouseEventData(wParam, lParam, isInjected);
                    eventArgs.PreviousState = InputStateManager.GetState(eventArgs.Key);
                    InputStateManager.SetState(eventArgs.Key, eventArgs.State);
                    Hooked?.Invoke(this, eventArgs);
                    if (eventArgs.WillCancel)
                    {
                        return (IntPtr)1;
                    }
                }
            }
            catch (Exception)
            {
            }

            // 次のフックプロシージャへ
            return Win32Api.CallNextHookEx(_hookHandle, nCode, wParam, ref lParam);
        }

        private static HookEventArgs CreateMouseEventData(uint wParam, MouseLowLevelHookStruct lParam, bool isInjected)
        {
#pragma warning disable CS8603
            return wParam switch
            {
                Win32Api.WM_LEFT_DOWN => new HookEventArgs { Key = Keys.LButton, State = true, IsInjected = isInjected },
                Win32Api.WM_LEFT_UP => new HookEventArgs { Key = Keys.LButton, State = false, IsInjected = isInjected },
                Win32Api.WM_RIGHT_DOWN => new HookEventArgs { Key = Keys.RButton, State = true, IsInjected = isInjected },
                Win32Api.WM_RIGHT_UP => new HookEventArgs { Key = Keys.RButton, State = false, IsInjected = isInjected },
                Win32Api.WM_MIDDLE_DOWN => new HookEventArgs { Key = Keys.MButton, State = true, IsInjected = isInjected },
                Win32Api.WM_MIDDLE_UP => new HookEventArgs { Key = Keys.MButton, State = false, IsInjected = isInjected },
                Win32Api.WM_XBUTTON_DOWN => (lParam.MouseData >> 16) == 1 ?
                    new HookEventArgs { Key = Keys.XButton1, State = true, IsInjected = isInjected }:
                    new HookEventArgs { Key = Keys.XButton2, State = true, IsInjected = isInjected },
                Win32Api.WM_XBUTTON_UP => (lParam.MouseData >> 16) == 1 ?
                    new HookEventArgs { Key = Keys.XButton1, State = false, IsInjected = isInjected }:
                    new HookEventArgs { Key = Keys.XButton2, State = false, IsInjected = isInjected },
                _ => null
            };
        }
    }
}
