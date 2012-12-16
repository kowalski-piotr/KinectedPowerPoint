using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Windows.Data;

namespace KinectedPowerPoint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(MainWindow),
                new PropertyMetadata(null));

        #region Private State
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        #endregion

        public MainWindow()
        {
            this.KinectSensorManager = new KinectSensorManager();
            this.KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;
            this.DataContext = this.KinectSensorManager;

            InitializeComponent();

            this.SensorChooserUI.KinectSensorChooser = sensorChooser;
            sensorChooser.Start();

            // Bind the KinectSensor from the sensorChooser to the KinectSensor on the KinectSensorManager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

        }

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }
        
        //KinectSensor kinectSensor;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;

            //    if (KinectSensor.KinectSensors.Count == 0)


            //    foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            //    {
            //        if (kinect.Status == KinectStatus.Connected)
            //        {
            //            kinectSensor = kinect;
            //            break;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        //void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        #region Kinect discovery + setup

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null != args.OldValue)
            {
                this.UninitializeKinectServices(args.OldValue);
            }

            if (null != args.NewValue)
            {
                this.InitializeKinectServices(this.KinectSensorManager, args.NewValue);
            }
        }

        // Kinect enabled apps should customize which Kinect services it initializes here.
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // Application should enable all streams first.
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            //sensor.SkeletonFrameReady += this.SkeletonsReady;
            kinectSensorManager.TransformSmoothParameters = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            kinectSensorManager.SkeletonStreamEnabled = true;
            kinectSensorManager.KinectSensorEnabled = true;
        }

        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            //sensor.SkeletonFrameReady -= this.SkeletonsReady;
        }

        #endregion Kinect discovery + setup
        

    }
}
