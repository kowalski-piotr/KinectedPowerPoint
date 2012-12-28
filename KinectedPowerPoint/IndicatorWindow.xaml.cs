using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Win32;
using Microsoft.Kinect;
using KinectedPowerPoint.Toolbox;

namespace KinectedPowerPoint
{
    public partial class IndicatorWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private KinectManager kinectManager;
        private SettingsWindow settingsWindow = new SettingsWindow();
        private SolidColorBrush GestureOn = Brushes.GreenYellow;
        private SolidColorBrush GestureOff = Brushes.LightGray;
        private SolidColorBrush KinectIssue = Brushes.LightCoral;
        private SolidColorBrush SkeletonNotTracked = Brushes.MediumVioletRed;
        
        #endregion 

        #region Properties
        public int CenterPosition 
        {
            get
            {
                return (int)((System.Windows.SystemParameters.FullPrimaryScreenWidth - this.Width) / 2);
            }
        }

        private SolidColorBrush statusColor;
        public SolidColorBrush StatusColor
        {
            get { return statusColor; }
            set
            {
                statusColor = value;
                NotifyPropertyChanged("StatusColor");
            }

        }

        private string statusTooltip;
        public string StatusTooltip
        {
            get { return statusTooltip; }
            set
            {
                statusTooltip = value;
                NotifyPropertyChanged("StatusTooltip");
            }

        }

        private bool topMost = true;
        public bool TopMost
        {
            get { return topMost; }
            set 
            { 
                topMost = value;
                NotifyPropertyChanged("TopMost");
            }
        }

        #endregion

        #region ctor
        public IndicatorWindow()
        {
            InitializeComponent();

            kinectManager = KinectManager.Instance;
            kinectManager.OnKinectStatusChanged    += kinectManager_OnKinectChanged;
            kinectManager.OnPostureDetectorChanged += kinectManager_OnPostureDetectorChanged;

            settingsWindow.DataContext = this;

            if (kinectManager.KinectStatus == KinectStatus.Connected)
            {
                StatusColor = GestureOff;
                StatusTooltip = "Kinect is connected";
            }
        }
        #endregion

        #region Status Event Handlers
        private void kinectManager_OnPostureDetectorChanged(Posture posture, bool isOn)
        {
            switch (posture)
            {
                case Posture.GesturesOn:
                    if (isOn)
                    {
                        StatusColor = GestureOn;
                        StatusTooltip = "Gestures detector is enabled";
                    }
                    else
                    {
                        StatusColor = GestureOff;
                        StatusTooltip = "Gestures detector is disabled";
                    }
                    break;
                case Posture.None:
                    StatusColor = SkeletonNotTracked;
                    StatusTooltip += "\nNo one in kinect range";
                    break;
            }
        }

        private void kinectManager_OnKinectChanged(KinectStatus kinectStatus)
        {
            if (kinectStatus != KinectStatus.Connected)
                StatusColor = KinectIssue;

            if (kinectStatus == KinectStatus.Connected &&
                kinectManager.IsGesturesOn)
                StatusColor = GestureOn;

            if (kinectStatus == KinectStatus.Connected &&
                !kinectManager.IsGesturesOn)
                StatusColor = GestureOff;

            switch (kinectStatus)
            {
                case KinectStatus.Connected:
                    StatusTooltip = "Kinect is connected";
                    break;
                case KinectStatus.Disconnected:
                    StatusTooltip = "Kinect was disconnected";
                    break;
                case KinectStatus.NotPowered:
                    StatusTooltip = "Kinect is no more powered";
                    break;
                case KinectStatus.NotReady:
                    StatusTooltip = "Kinect is not ready";
                    break;
                default:
                    StatusTooltip = kinectStatus.ToString();
                    break;
            }
        }
        #endregion

        #region Menu Event Handlers
        private void OnMenuClick(object sender, RoutedEventArgs e)
        {
            if (settingsWindow.IsVisible)
                settingsWindow.Hide();
            else
                settingsWindow.Show();
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            string fileName = getFilePath();
            if (!string.IsNullOrEmpty(fileName))
                kinectManager.OpenPresentation(fileName);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            kinectManager.ClosePresentation();
            Application.Current.Shutdown();
        }

        private void indicatorWin_Closing(object sender, CancelEventArgs e)
        {
            kinectManager.KinectSensor.Stop();
            kinectManager.KinectSensor = null;
        }
        #endregion

        #region FileDialog 
        private string getFilePath()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".pptx";
            dialog.Filter = "PowerPoint Files (*.pptx;*.ppt)|*.pptx;*.ppt";

            bool? result = dialog.ShowDialog();
            if (result == true)
                return dialog.FileName;
            else
                return string.Empty;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
