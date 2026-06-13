using System;
using System.Windows.Forms;

namespace WindowStacker
{
    internal class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int HOTKEY_BRING_FORWARD = 1;
        private const int HOTKEY_SEND_BACK     = 2;
        private const int HOTKEY_CLOSE_WINDOW  = 3;

        public event Action? BringForward;
        public event Action? SendBack;
        public event Action? CloseWindow;

        private bool _f1Registered;
        private bool _f3Registered;
        private bool _escRegistered;
        private bool _disposed;

        public HotkeyWindow()
        {
            var cp = new CreateParams
            {
                Caption = "WindowStackerHotkeyReceiver",
                ExStyle = 0x00000080  // WS_EX_TOOLWINDOW
            };
            CreateHandle(cp);

            _escRegistered = NativeMethods.RegisterHotKey(
                Handle,
                HOTKEY_CLOSE_WINDOW,
                NativeMethods.MOD_ALT,
                NativeMethods.VK_ESCAPE);

            if (!_escRegistered)
                MessageBox.Show(
                    "Failed to register Alt+Esc.\nAnother application may be using it.",
                    "WindowStacker",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
        }

        public bool SetF1Enabled(bool enabled)
        {
            if (enabled == _f1Registered) return true;
            if (enabled)
            {
                _f1Registered = NativeMethods.RegisterHotKey(
                    Handle,
                    HOTKEY_BRING_FORWARD,
                    NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT,
                    NativeMethods.VK_F1);
                return _f1Registered;
            }
            else
            {
                NativeMethods.UnregisterHotKey(Handle, HOTKEY_BRING_FORWARD);
                _f1Registered = false;
                return true;
            }
        }

        public bool SetF3Enabled(bool enabled)
        {
            if (enabled == _f3Registered) return true;
            if (enabled)
            {
                _f3Registered = NativeMethods.RegisterHotKey(
                    Handle,
                    HOTKEY_SEND_BACK,
                    NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT,
                    NativeMethods.VK_F3);
                return _f3Registered;
            }
            else
            {
                NativeMethods.UnregisterHotKey(Handle, HOTKEY_SEND_BACK);
                _f3Registered = false;
                return true;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_BRING_FORWARD: BringForward?.Invoke(); return;
                    case HOTKEY_SEND_BACK:     SendBack?.Invoke();     return;
                    case HOTKEY_CLOSE_WINDOW:  CloseWindow?.Invoke();  return;
                }
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (Handle != IntPtr.Zero)
            {
                if (_f1Registered)  NativeMethods.UnregisterHotKey(Handle, HOTKEY_BRING_FORWARD);
                if (_f3Registered)  NativeMethods.UnregisterHotKey(Handle, HOTKEY_SEND_BACK);
                if (_escRegistered) NativeMethods.UnregisterHotKey(Handle, HOTKEY_CLOSE_WINDOW);
            }

            DestroyHandle();
        }
    }
}
