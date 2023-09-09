using KinectMouseV1.Utilities;
using Microsoft.Kinect;
using System;
using System.Windows.Forms;

namespace KinectMouseV1
{
    internal class MouseController : IDisposable
    {
        private readonly KinectHandler _kinectHandler;

        private bool _running = false;

        public MouseController(MainApplicationContext context)
        {
            _kinectHandler = new(context);
            _kinectHandler.SetOnHandPositionChange(OnHandPositionChange);
            _kinectHandler.SetOnHandClosedChange(OnHandClosedChange);
        }

        public void ToggleRunning()
        {
            _running = !_running;
        }

        public void Execute()
        {
            _kinectHandler.Start();
        }

        private void OnHandClosedChange(bool closed)
        {
            if (closed)
            {
                MouseHook.MouseDown();
            }
            else
            {
                MouseHook.MouseUp();
            }
        }

        private void OnHandPositionChange(SkeletonPoint pos)
        {
            // Conversion
            var mapper = _kinectHandler.GetCoordinateMapper();
            var point = mapper.MapSkeletonPointToDepthPoint(pos, DepthImageFormat.Resolution640x480Fps30);

            // Scaling
            var screenWidth = Screen.PrimaryScreen.Bounds.Width;
            var screenHeight = Screen.PrimaryScreen.Bounds.Height;
            var scaledX = (float)point.X / 640 * screenWidth;
            var scaledY = (float)point.Y / 480 * screenHeight;

            MouseHook.SetCursorPos((int)scaledX, (int)scaledY);
        }

        public void Dispose()
        {
            _kinectHandler.Dispose();
        }
    }
}
