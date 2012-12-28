using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Kinect;
using System;

namespace KinectedPowerPoint.Toolbox
{
    class ContextTracker
    {
        readonly Dictionary<int, List<ContextPoint>> points = new Dictionary<int, List<ContextPoint>>();

        public float Threshold { get; set; }

        public ContextTracker(float threshold = 0.05f)
        {
            Threshold = threshold;
        }

        public void Add(Vector3 position, int trackingID)
        {
            if (!points.ContainsKey(trackingID))
                points.Add(trackingID, new List<ContextPoint>());

            points[trackingID].Add(new ContextPoint() { Position = position, Time = DateTime.Now });
        }

        public bool IsStableRelativeToCurrentSpeed(int trackingID)
        {
            List<ContextPoint> currentPoints = points[trackingID];
            if (currentPoints.Count < 2)
                return false;

            Vector3 previousPosition = currentPoints[currentPoints.Count - 2].Position;
            Vector3 currentPosition = currentPoints[currentPoints.Count - 1].Position;
            DateTime previousTime = currentPoints[currentPoints.Count - 2].Time;
            DateTime currentTime = currentPoints[currentPoints.Count - 1].Time;

            var currentSpeed = (currentPosition - previousPosition).Length / ((currentTime - previousTime).TotalMilliseconds);

            if (currentSpeed > Threshold)
                return false;

            return true;
        }
    }
}
