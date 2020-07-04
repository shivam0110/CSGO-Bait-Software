using ScriptKidAntiCheat.Win32;
using ScriptKidAntiCheat.Win32.Data;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

/*
 * Credit: https://stackoverflow.com/questions/11607133/global-mouse-event-handler
 * */
namespace ScriptKidAntiCheat.Utils
{
    public static class MouseHook
    {
        public static event EventHandler MouseAction = delegate { };

        public static void Start()
        {
            _hookID = SetHook(_proc);
        }
        public static void stop()
        {
            User32.UnhookWindowsHookEx(_hookID);
        }

        private static User32.LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(User32.LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return User32.SetWindowsHookEx(WH_MOUSE_LL, proc, User32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (MouseEvents)wParam != MouseEvents.WM_MOUSEMOVE)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                MouseAction((MouseEvents)wParam, new EventArgs());
            }

            return User32.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        public enum MouseEvents
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

    }

}
