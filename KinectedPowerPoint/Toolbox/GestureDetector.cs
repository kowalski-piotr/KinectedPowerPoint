using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Kinect;

namespace KinectedPowerPoint.Toolbox
{
    public abstract class GestureDetector
    {
        public event Action<Gesture, JointType> OnGestureDetected;

        public int MinimalPeriodBetweenGestures { get; set; }
        public JointType CurrentJointType{ get; set; }
        
        private readonly Dictionary<JointType, List<Entry>> entries = new Dictionary<JointType, List<Entry>>();
        protected Dictionary<JointType, List<Entry>> Entries
        {
            get { return entries; }
        }

        public virtual void Add(Joint joint)
        {
            Entry newEntry = new Entry { Position = joint.Position.ToVector3(), Time = DateTime.Now };
            CurrentJointType = joint.JointType;

            if (Entries.ContainsKey(joint.JointType))
                Entries[CurrentJointType].Add(newEntry);
            else
                Entries.Add(CurrentJointType, new List<Entry>() { newEntry });

            LookForGesture();
        }

        protected abstract void LookForGesture();

        protected void RaiseGestureDetected(Gesture gesture, JointType jointType)
        {
            if (OnGestureDetected != null)
                OnGestureDetected(gesture, jointType);
        }
    }

    public enum Gesture
    {
        None,
        SwipeToRight,
        SwipeToLeft
    }
}
