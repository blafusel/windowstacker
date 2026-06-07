using System;

namespace WindowStacker
{
    internal static class WindowZOrder
    {
        private static readonly uint FLAGS =
            NativeMethods.SWP_NOMOVE |
            NativeMethods.SWP_NOSIZE |
            NativeMethods.SWP_NOACTIVATE |
            NativeMethods.SWP_ASYNCWINDOWPOS;

        /// <summary>
        /// Returns the top-level window under the current mouse cursor,
        /// or IntPtr.Zero if none is found or the window is not usable.
        /// </summary>
        public static IntPtr GetWindowUnderCursor()
        {
            if (!NativeMethods.GetCursorPos(out var point))
                return IntPtr.Zero;

            IntPtr hwnd = NativeMethods.WindowFromPoint(point);
            if (hwnd == IntPtr.Zero)
                return IntPtr.Zero;

            // Walk up to the top-level owner so we act on the real window,
            // not a child control buried inside it.
            IntPtr root = NativeMethods.GetAncestor(hwnd, NativeMethods.GA_ROOTOWNER);
            if (root == IntPtr.Zero)
                root = hwnd;

            if (!NativeMethods.IsWindow(root) || !NativeMethods.IsWindowVisible(root))
                return IntPtr.Zero;

            return root;
        }

        public static void BringToFront()
        {
            IntPtr hwnd = GetWindowUnderCursor();
            if (hwnd == IntPtr.Zero) return;

            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOP, 0, 0, 0, 0, FLAGS);
        }

        public static void SendToBack()
        {
            IntPtr hwnd = GetWindowUnderCursor();
            if (hwnd == IntPtr.Zero) return;

            NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_BOTTOM, 0, 0, 0, 0, FLAGS);
        }

        public static void CloseWindowUnderCursor()
        {
            IntPtr hwnd = GetWindowUnderCursor();
            if (hwnd == IntPtr.Zero) return;

            NativeMethods.PostMessage(hwnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static void CloseActiveWindow()
        {
            IntPtr hwnd = NativeMethods.GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return;

            NativeMethods.PostMessage(hwnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
