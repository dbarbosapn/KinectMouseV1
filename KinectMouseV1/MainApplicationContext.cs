using KinectMouseV1.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace KinectMouseV1
{
    /// <summary>
    /// Main context for the application being run.
    /// Sets up the tray icon and events.
    /// </summary>
    internal class MainApplicationContext : ApplicationContext
    {
        private readonly MouseController _controller;

        public NotifyIcon Icon { get; }

        public MainApplicationContext()
        {
            Icon = new()
            {
                Icon = new Icon("icon.ico"),
                ContextMenuStrip = new(),
                Visible = true
            };
            Icon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, ExitApplication));

            var hook = new KeyboardHook();
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(OnHotKeyPressed);
            hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift, Keys.E);

            _controller = new MouseController(this);
            _controller.Execute();
        }

        private void OnHotKeyPressed(object sender, KeyPressedEventArgs e)
        {
            _controller.ToggleRunning();
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            Icon.Visible = false;
            Application.Exit();
        }
    }
}
