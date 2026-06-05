using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowStacker
{
    internal class TrayApplication : IDisposable
    {
        private readonly NotifyIcon _trayIcon;
        private readonly HotkeyWindow _hotkeyWindow;
        private bool _enabled = true;
        private bool _disposed;

        public TrayApplication()
        {
            _hotkeyWindow = new HotkeyWindow();
            _hotkeyWindow.BringForward += OnBringForward;
            _hotkeyWindow.SendBack     += OnSendBack;

            _trayIcon = new NotifyIcon
            {
                Icon    = BuildTrayIcon(),
                Text    = "WindowStacker\nAlt+F1: Bring forward\nAlt+F3: Send back",
                Visible = true,
                ContextMenuStrip = BuildContextMenu()
            };

            _trayIcon.DoubleClick += (_, _) => ToggleEnabled();
        }

        private void OnBringForward()
        {
            if (_enabled) WindowZOrder.BringToFront();
        }

        private void OnSendBack()
        {
            if (_enabled) WindowZOrder.SendToBack();
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
                ? "WindowStacker\nAlt+F1: Bring forward\nAlt+F3: Send back"
                : "WindowStacker (paused)\nDouble-click to resume";

            // Refresh the menu item label
            if (_trayIcon.ContextMenuStrip?.Items[0] is ToolStripMenuItem item)
                item.Text = _enabled ? "Pause" : "Resume";
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            var toggleItem = new ToolStripMenuItem("Pause");
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

        /// <summary>
        /// Draws a simple programmatic icon so we have no external resource dependency.
        /// A small "W" on a dark background, greyed out when paused.
        /// </summary>
        private Icon BuildTrayIcon()
        {
            var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);

            Color bg   = _enabled ? Color.FromArgb(30, 30, 30)   : Color.FromArgb(80, 80, 80);
            Color text = _enabled ? Color.FromArgb(220, 220, 220) : Color.FromArgb(140, 140, 140);

            g.Clear(bg);

            using var font  = new Font("Arial", 7f, System.Drawing.FontStyle.Bold);
            using var brush = new SolidBrush(text);

            // Centre the "W" glyph
            var size   = g.MeasureString("W", font);
            float x    = (16 - size.Width)  / 2f;
            float y    = (16 - size.Height) / 2f;
            g.DrawString("W", font, brush, x, y);

            return Icon.FromHandle(bmp.GetHicon());
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _hotkeyWindow.BringForward -= OnBringForward;
            _hotkeyWindow.SendBack     -= OnSendBack;
            _hotkeyWindow.Dispose();

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
    }
}
