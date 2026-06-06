using System;
using System.Windows.Forms;

namespace WindowStacker
{
    /// <summary>
    /// Invisible window used solely to receive WM_HOTKEY messages.
    /// WinForms needs a real HWND to pass to RegisterHotKey.
    /// </summary>
    internal class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int HOTKEY_BRING_FORWARD = 1;
        private const int HOTKEY_SEND_BACK     = 2;
        private const int HOTKEY_CLOSE_WINDOW  = 3;

        public event Action? BringForward;
        public event Action? SendBack;
        public event Action? CloseWindow;

        private bool _registered;
        private bool _disposed;

        public HotkeyWindow()
        {
            // Create a minimal hidden window
            var cp = new CreateParams
            {
                Caption = "WindowStackerHotkeyReceiver",
                // WS_EX_TOOLWINDOW keeps it out of the taskbar and alt-tab
                ExStyle = 0x00000080
            };
            CreateHandle(cp);
            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            bool f1Ok  = NativeMethods.RegisterHotKey(
                Handle,
                HOTKEY_BRING_FORWARD,
                NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT,
                NativeMethods.VK_F1);

            bool f3Ok  = NativeMethods.RegisterHotKey(
                Handle,
                HOTKEY_SEND_BACK,
                NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT,
                NativeMethods.VK_F3);

            // MOD_ALT alone on VK_ESCAPE overrides the Windows/PowerToys Alt+Esc system hotkey.
            // MOD_NOREPEAT intentionally omitted so the registration doesn't silently fail on
            // systems where that flag conflicts with the system hook for this key.
            bool escOk = NativeMethods.RegisterHotKey(
                Handle,
                HOTKEY_CLOSE_WINDOW,
                NativeMethods.MOD_ALT,
                NativeMethods.VK_ESCAPE);

            if (!f1Ok || !f3Ok || !escOk)
            {
                MessageBox.Show(
                    "Failed to register one or more hotkeys.\n\n" +
                    $"Alt+F1:  {(f1Ok  ? "OK" : "FAILED")}\n" +
                    $"Alt+F3:  {(f3Ok  ? "OK" : "FAILED")}\n" +
                    $"Alt+Esc: {(escOk ? "OK" : "FAILED")}\n\n" +
                    "Another application may be using these shortcuts.",
                    "WindowStacker",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            _registered = f1Ok || f3Ok || escOk;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_BRING_FORWARD:
                        BringForward?.Invoke();
                        return;
                    case HOTKEY_SEND_BACK:
                        SendBack?.Invoke();
                        return;
                    case HOTKEY_CLOSE_WINDOW:
                        CloseWindow?.Invoke();
                        return;
                }
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_registered && Handle != IntPtr.Zero)
            {
                NativeMethods.UnregisterHotKey(Handle, HOTKEY_BRING_FORWARD);
                NativeMethods.UnregisterHotKey(Handle, HOTKEY_SEND_BACK);
                NativeMethods.UnregisterHotKey(Handle, HOTKEY_CLOSE_WINDOW);
            }

            DestroyHandle();
        }
    }
}
