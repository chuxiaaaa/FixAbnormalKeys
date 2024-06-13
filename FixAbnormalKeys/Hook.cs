using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FixAbnormalKeys
{
    public class Hook
    {
        public event KeyEventCallBack KeyDown;

        public event KeyEventCallBack KeyUp;

        HookProc HookProcedure;

        int hHook;

        public delegate bool KeyEventCallBack(bool KeyDown, KeyEventArgs e);

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        public bool Start()
        {
            if (hHook == 0)
            {
                HookProcedure = new HookProc(KeyboardHookProc);
                try
                {
                    hHook = SetWindowsHookEx(13,
                        HookProcedure,
                        Marshal.GetHINSTANCE(
                        Assembly.GetExecutingAssembly().GetModules()[0]),
                        0);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public void Stop()
        {
            if (hHook != 0)
            {
                UnhookWindowsHookEx(hHook);
            }
        }

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (KeyDown != null || KeyUp != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct =
                    (KeyboardHookStruct)Marshal.PtrToStructure(lParam,
                    typeof(KeyboardHookStruct));
                if (KeyDown != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    var ret = KeyDown(true, new KeyEventArgs((Keys)MyKeyboardHookStruct.vkCode));
                    if (ret)
                    {
                        return 1;
                    }
                }
                if (KeyUp != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    var ret = KeyUp(false, new KeyEventArgs((Keys)MyKeyboardHookStruct.vkCode));
                    if (ret)
                    {
                        return 1;
                    }
                }
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
}
