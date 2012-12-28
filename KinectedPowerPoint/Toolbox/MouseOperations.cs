using System;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.Kinect;

namespace KinectedPowerPoint.Toolbox
{
    public class MouseOperations
    {
        #region Interop
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);
        #endregion

        internal static void SetCursorPosition(Joint joint)
        {
            int x = (int)Scale((int)SystemParameters.PrimaryScreenWidth, 0.6f, joint.Position.X);
            int y = (int)Scale((int)SystemParameters.PrimaryScreenHeight, 0.4f, -joint.Position.Y);
            SetCursorPos(x, y);
        }

        private static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint)  
                currentMousePoint = new MousePoint(0, 0); 

            return currentMousePoint;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = x;
            }
        }

        private static float Scale(int maxPixel, float maxSkeleton, float position)
        {
            float value = ((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));
            if (value > maxPixel)
                return maxPixel;
            if (value < 0)
                return 0;

            return value;
        }
    }
}
