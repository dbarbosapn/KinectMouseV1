using System;
using System.Windows.Forms;

namespace KinectMouseV1
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new MainApplicationContext());
        }
    }
}
