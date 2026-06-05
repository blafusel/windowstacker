using System;
using System.Runtime.InteropServices;

namespace WindowStacker
{
    internal static class NativeMethods
    {
        // Hotkey registration
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Mouse position
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        // Window from point
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT point);

        // Walk up to top-level window
        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        // Z-order manipulation
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        // Constants for SetWindowPos hWndInsertAfter
        public static readonly IntPtr HWND_TOP    = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        // SetWindowPos flags
        public const uint SWP_NOMOVE     = 0x0002;
        public const uint SWP_NOSIZE     = 0x0001;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_ASYNCWINDOWPOS = 0x4000;

        // GetAncestor flag: get root (top-level) owner
        public const uint GA_ROOTOWNER = 3;

        // Hotkey modifiers
        public const uint MOD_ALT     = 0x0001;
        public const uint MOD_NOREPEAT = 0x4000;

        // Virtual key codes
        public const uint VK_F1 = 0x70;
        public const uint VK_F3 = 0x72;

        // Window message for hotkeys
        public const int WM_HOTKEY = 0x0312;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
