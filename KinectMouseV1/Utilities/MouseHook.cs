using System;
using System.Runtime.InteropServices;

namespace KinectMouseV1.Utilities
{
    internal static class MouseHook
    {
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);

        public static void MouseDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 50, 50, 0, (UIntPtr)0);
        }

        public static void MouseUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 50, 50, 0, (UIntPtr)0);
        }
    }
}
