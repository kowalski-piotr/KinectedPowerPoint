using System;
using Microsoft.Kinect;
using System.Linq;
using System.Collections.Generic;

namespace KinectedPowerPoint.Toolbox
{
    public class SwipeGestureDetector : GestureDetector
    {
        public float SwipeMinimalLength { get; set; }
        public float SwipeMaximalHeight { get; set; }
        public int SwipeMinimalDuration { get; set; }
        public int SwipeMaximalDuration { get; set; }
        public DateTime LastGestureTime { get; set; }

        public SwipeGestureDetector()
        {
            SwipeMinimalLength = 0.3f;
            SwipeMaximalHeight = 0.4f;
            SwipeMinimalDuration = 50;
            SwipeMaximalDuration = 400;
            MinimalPeriodBetweenGestures = 0;
            LastGestureTime = DateTime.Now;
        }

        protected bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction,
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            List<Entry> entries = Entries[CurrentJointType];

            for (int index = 1; index < entries.Count - 1; index++)
            {
                if (!heightFunction(entries[0].Position, entries[index].Position) ||
                    !directionFunction(entries[index].Position, entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(entries[index].Position, entries[start].Position))
                {
                    double totalMilliseconds = (entries[index].Time - entries[start].Time).TotalMilliseconds;
                    double periodBetweenGestures =  DateTime.Now.Subtract(LastGestureTime).TotalMilliseconds;

                    if (totalMilliseconds >= minTime && 
                        totalMilliseconds <= maxTime &&
                        periodBetweenGestures > MinimalPeriodBetweenGestures)
                    {
                        LastGestureTime = DateTime.Now;
                        Entries.Clear();
                        return true;
                    }
                }
            }

            if (Entries[CurrentJointType].Count > 30)
                Entries[CurrentJointType].Clear();

            return false;
        }

        protected override void LookForGesture()
        {
            // Swipe to right
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
                (p1, p2) => p2.X - p1.X > -0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMinimalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected(Gesture.SwipeToRight, CurrentJointType);
                return;
            }

            // Swipe to left
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
                (p1, p2) => p2.X - p1.X < 0.01f, // Progression to left
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMinimalDuration, SwipeMaximalDuration))// Duration
            {
                RaiseGestureDetected(Gesture.SwipeToLeft, CurrentJointType);
                return;
            }
        }
    }
}
