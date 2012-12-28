using System;
using System.Windows;
using Microsoft.Kinect;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace KinectedPowerPoint
{
    public partial class SettingsWindow : Window
    {
        #region Properties
        public int CenterPosition
        {
            get
            {
                return (int)((System.Windows.SystemParameters.FullPrimaryScreenWidth - this.Width) / 2);
            }
        }

        private int cameraAngle = KinectManager.Instance.CameraAngle;
        public int CameraAngle
        {
            get { return cameraAngle; }
            set { cameraAngle = value; }
        }

        private int gestureDuration = KinectManager.Instance.GestureDuration;
        public int GestureDuration
        {
            get { return gestureDuration; }
            set { gestureDuration = value; }
        }

        private float gestureLength = KinectManager.Instance.GestureLength;
        public float GestureLength
        {
            get { return gestureLength; }
            set { gestureLength = value; }
        }
        #endregion

        #region ctor
        public SettingsWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Settings Event Handlers
        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
            KinectManager.Instance.CameraAngle = CameraAngle;
            KinectManager.Instance.GestureDuration = GestureDuration;
            KinectManager.Instance.GestureLength = GestureLength;
        }
        #endregion
    }
}
