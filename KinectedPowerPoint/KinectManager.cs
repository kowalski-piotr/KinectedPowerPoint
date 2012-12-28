using System;
using Microsoft.Kinect;
using KinectedPowerPoint.Toolbox;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace KinectedPowerPoint
{
    public sealed class KinectManager
    {
        #region Fields
        public KinectSensor KinectSensor;
        private Skeleton[] skeletons;
        private readonly ContextTracker contextTracker = new ContextTracker();
        private SwipeGestureDetector swipeGestureRecognizer;
        private InitiatePostureDetector initiatePostureDetector;

        private PowerPoint.Presentation ppPresentation;
        private PowerPoint.Application ppApplication;
        #endregion

        #region Flags
        public bool IsPresentationMode = false;
        #endregion

        #region Actions
        public event Action<KinectStatus> OnKinectStatusChanged;
        public event Action<Posture, bool> OnPostureDetectorChanged;
        #endregion

        #region Properties
        private KinectStatus kinectStatus;
        public KinectStatus KinectStatus
        {
            get { return kinectStatus; }
            set
            {
                kinectStatus = value;
                if (OnKinectStatusChanged != null)
                    OnKinectStatusChanged(value);
            }
        }

        private bool isGesturesOn = false;
        public bool IsGesturesOn
        {
            get { return isGesturesOn; }
            set
            {
                isGesturesOn = value;
                if (OnPostureDetectorChanged != null)
                    OnPostureDetectorChanged(Posture.GesturesOn, value);
            }
        }

        private bool isCatchCursor;
        public bool IsCatchCursor
        {
            get { return isCatchCursor; }
            set 
            {
                isCatchCursor = value;
                if (OnPostureDetectorChanged != null)
                    OnPostureDetectorChanged(Posture.CatchCursor, value);
            }
        }
        
        public int CameraAngle 
        { 
            get
            {
                if (KinectSensor != null)
                    return KinectSensor.ElevationAngle;
                return 0;
            }
            set
            {
                if (KinectSensor != null && value >= -27 && value <= 27)
                    KinectSensor.ElevationAngle = value;
            }
        }

        public int GestureDuration
        {
            get
            {
                if (swipeGestureRecognizer != null)
                    return swipeGestureRecognizer.SwipeMaximalDuration;
                return 0;
            }
            set
            {
                if (swipeGestureRecognizer != null)
                    swipeGestureRecognizer.SwipeMaximalDuration = value;
            }
        }

        public float GestureLength
        {
            get
            {
                if (swipeGestureRecognizer != null)
                    return swipeGestureRecognizer.SwipeMinimalLength;
                return 0;
            }
            set
            {
                if (swipeGestureRecognizer != null)
                    swipeGestureRecognizer.SwipeMinimalLength = value;
            }
        }
        #endregion

        #region Singleton
        private readonly static KinectManager instance = new KinectManager();
        public static KinectManager Instance
        {
            get { return instance; }
        }
        #endregion

        #region ctor
        private KinectManager()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;

            ppApplication = new PowerPoint.Application();
            ppApplication.AfterPresentationOpen += ppApplication_AfterPresentationOpen;
            ppApplication.SlideShowEnd += ppApplication_SlideShowEnd;

            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                if (kinect.Status == KinectStatus.Connected)
                {
                    KinectStatus = kinect.Status;
                    KinectSensor = kinect;
                    break;
                }
            }

            if (KinectSensor.KinectSensors.Count == 0)
                MessageBox.Show("No Kinect found");
            else
                InitializeKinectServices();

        }
        #endregion

        #region kinect setup
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            KinectStatus = e.Status;

            if (e.Status == Microsoft.Kinect.KinectStatus.Connected && KinectSensor == null)
            {
                KinectSensor = e.Sensor;
                InitializeKinectServices();
            }
            else
                UninitializeKinectServices();
        }

        private void InitializeKinectServices()
        {
            KinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
            {
                Smoothing = 0.2f,
                Correction = 0.3f,
                Prediction = 0.1f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.05f
            });
            KinectSensor.SkeletonFrameReady += kinectSensor_SkeletonFrameReady;

            swipeGestureRecognizer = new SwipeGestureDetector();
            initiatePostureDetector = new InitiatePostureDetector();

            swipeGestureRecognizer.OnGestureDetected += swipeGestureRecognizer_OnGestureDetected;
            initiatePostureDetector.OnPostureDetected  += initiatePostureDetector_OnPostureDetected;
            KinectSensor.Start();

        }

        private void UninitializeKinectServices()
        {
            if (KinectSensor != null)
            {
                KinectSensor.SkeletonFrameReady -= kinectSensor_SkeletonFrameReady;
                swipeGestureRecognizer.OnGestureDetected -= swipeGestureRecognizer_OnGestureDetected;
                initiatePostureDetector.OnPostureDetected -= initiatePostureDetector_OnPostureDetected;
                KinectSensor.Stop();
                KinectSensor = null;
            }
        }
        #endregion

        #region Gestures & Postures
        private void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                frame.GetSkeletons(ref skeletons);

                if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;

                foreach (var skeleton in skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                        continue;

                    contextTracker.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);

                    if (!contextTracker.IsStableRelativeToCurrentSpeed(skeleton.TrackingId))
                        continue;

                    initiatePostureDetector.TrackPostures(skeleton);

                    if (IsCatchCursor)
                    {
                        
                        MouseOperations.SetCursorPosition(skeleton.Joints.Single(x => x.JointType == JointType.HandRight));
                    }

                    if (IsGesturesOn)
                    {
                        foreach (Joint joint in skeleton.Joints)
                        {
                            if (joint.TrackingState != JointTrackingState.Tracked)
                                continue;

                            if (joint.JointType == JointType.HandLeft || joint.JointType == JointType.HandRight)
                                swipeGestureRecognizer.Add(joint);
                        }
                    }

                }
            }
        }

        private void initiatePostureDetector_OnPostureDetected(Posture posture)
        {
            if (IsPresentationMode)
            {
                switch (posture)
                {
                    case Posture.GesturesOn:
                        IsGesturesOn = !IsGesturesOn;
                        break;
                    case Posture.CatchCursor:
                        IsCatchCursor = !IsCatchCursor;
                        break;
                }
            }
        }

        private void swipeGestureRecognizer_OnGestureDetected(Gesture gesture, JointType jointType)
        {
            if (gesture == Gesture.SwipeToRight && jointType == JointType.HandRight)
            {
                ppPresentation.SlideShowWindow.View.Next();
                return;
            }

            if (gesture == Gesture.SwipeToLeft && jointType == JointType.HandLeft)
            {
                ppPresentation.SlideShowWindow.View.Previous();
                return;
            }
        }
        #endregion

        #region Presentation
        private void ppApplication_AfterPresentationOpen(PowerPoint.Presentation Pres)
        {
            IsPresentationMode = true;
        }

        private void ppApplication_SlideShowEnd(PowerPoint.Presentation Pres)
        {
            IsPresentationMode = false;
            IsGesturesOn = false;
            ClosePresentation();
        }

        public void OpenPresentation(string fileName)
        {
            if(!string.IsNullOrEmpty(fileName))
            {
                PowerPoint.Presentations pres = ppApplication.Presentations;
                ppPresentation = pres.Open(fileName, MsoTriState.msoTrue, MsoTriState.msoTrue, MsoTriState.msoFalse);
                ppPresentation.SlideShowSettings.Run();
            }
        }

        public void ClosePresentation()
        {
            if (IsPresentationMode)
                ppPresentation.SlideShowWindow.View.Exit();
        }
        #endregion

    }
}
