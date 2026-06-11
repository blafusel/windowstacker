using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowStacker
{
    internal class TrayApplication : IDisposable
    {
        private readonly NotifyIcon _trayIcon;
        private readonly HotkeyWindow _hotkeyWindow;
        private bool _enabled = true;
        private bool _disposed;

        // Input tracking for smart Alt+Esc: true = last action was mouse move
        private bool _lastActionWasMouseMove;
        private bool _lmbDown;
        private IntPtr _mouseHook;
        private IntPtr _keyboardHook;
        // Keep delegates alive to prevent GC collection while hooks are active
        private NativeMethods.LowLevelHookProc? _mouseHookProc;
        private NativeMethods.LowLevelHookProc? _keyboardHookProc;

        public TrayApplication()
        {
            _hotkeyWindow = new HotkeyWindow();
            _hotkeyWindow.BringForward += OnBringForward;
            _hotkeyWindow.SendBack     += OnSendBack;
            _hotkeyWindow.CloseWindow  += OnCloseWindow;

            _trayIcon = new NotifyIcon
            {
                Icon    = BuildTrayIcon(),
                Text    = "WindowStacker\nAlt+F1: Bring forward\nAlt+F3: Send back\nAlt+Esc: Close",
                Visible = true,
                ContextMenuStrip = BuildContextMenu()
            };

            _trayIcon.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ToggleEnabled(); };

            InstallInputHooks();
        }

        private void InstallInputHooks()
        {
            _mouseHookProc    = MouseHookCallback;
            _keyboardHookProc = KeyboardHookCallback;
            IntPtr hMod = NativeMethods.GetModuleHandle(null);
            _mouseHook    = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL,    _mouseHookProc,    hMod, 0);
            _keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _keyboardHookProc, hMod, 0);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == NativeMethods.WM_MOUSEMOVE)
                {
                    _lastActionWasMouseMove = true;
                }
                else if (msg == NativeMethods.WM_LBUTTONDOWN)
                {
                    _lmbDown = true;
                    _lastActionWasMouseMove = false;
                }
                else if (msg == NativeMethods.WM_LBUTTONUP)
                {
                    _lmbDown = false;
                }
                else if (msg == NativeMethods.WM_RBUTTONDOWN)
                {
                    if (_enabled && _lmbDown)
                        WindowZOrder.SendActiveWindowToBack();
                    _lastActionWasMouseMove = false;
                }
                else if (msg == NativeMethods.WM_MBUTTONDOWN || msg == NativeMethods.WM_XBUTTONDOWN)
                {
                    _lastActionWasMouseMove = false;
                }
            }
            return NativeMethods.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN)
                {
                    var kbd = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                    // Exclude Alt and Esc so the hotkey itself doesn't corrupt the flag
                    if (kbd.vkCode != NativeMethods.VK_MENU && kbd.vkCode != NativeMethods.VK_ESCAPE)
                        _lastActionWasMouseMove = false;
                }
            }
            return NativeMethods.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        private void OnBringForward()
        {
            if (_enabled) WindowZOrder.BringToFront();
        }

        private void OnSendBack()
        {
            if (_enabled) WindowZOrder.SendToBack();
        }

        private void OnCloseWindow()
        {
            if (!_enabled) return;
            if (_lastActionWasMouseMove)
                WindowZOrder.CloseWindowUnderCursor();
            else
                WindowZOrder.CloseActiveWindow();
        }

        private void ToggleEnabled()
        {
            _enabled = !_enabled;
            UpdateTrayState();
        }

        private void UpdateTrayState()
        {
            _trayIcon.Icon = BuildTrayIcon();
            _trayIcon.Text = _enabled
                ? "WindowStacker\nAlt+F1: Bring forward\nAlt+F3: Send back\nAlt+Esc: Close"
                : "WindowStacker (disabled)\nLeft-click to enable";

            if (_trayIcon.ContextMenuStrip?.Items[0] is ToolStripMenuItem item)
                item.Text = _enabled ? "Disable" : "Enable";
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            var toggleItem = new ToolStripMenuItem("Disable");
            toggleItem.Click += (_, _) => ToggleEnabled();

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (_, _) =>
            {
                Dispose();
                Application.Exit();
            };

            menu.Items.Add(toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            return menu;
        }

        private Icon BuildTrayIcon()
        {
            var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);

            Color bg   = _enabled ? Color.FromArgb(30, 30, 30)   : Color.FromArgb(90, 90, 90);
            Color text = _enabled ? Color.FromArgb(220, 220, 220) : Color.FromArgb(160, 160, 160);

            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = RoundedRect(0, 0, 15, 15, 3f))
            using (var bgBrush = new SolidBrush(bg))
                g.FillPath(bgBrush, path);

            g.SmoothingMode = SmoothingMode.Default;

            using var font  = new Font("Arial", 7f, FontStyle.Bold);
            using var brush = new SolidBrush(text);

            var size = g.MeasureString("W", font);
            float x  = (16 - size.Width)  / 2f;
            float y  = (16 - size.Height) / 2f;
            g.DrawString("W", font, brush, x, y);

            return Icon.FromHandle(bmp.GetHicon());
        }

        private static GraphicsPath RoundedRect(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            path.AddArc(x,             y,             r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y,             r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2,   0, 90);
            path.AddArc(x,             y + h - r * 2, r * 2, r * 2,  90, 90);
            path.CloseFigure();
            return path;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_mouseHook    != IntPtr.Zero) NativeMethods.UnhookWindowsHookEx(_mouseHook);
            if (_keyboardHook != IntPtr.Zero) NativeMethods.UnhookWindowsHookEx(_keyboardHook);

            _hotkeyWindow.BringForward -= OnBringForward;
            _hotkeyWindow.SendBack     -= OnSendBack;
            _hotkeyWindow.CloseWindow  -= OnCloseWindow;
            _hotkeyWindow.Dispose();

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
    }
}
