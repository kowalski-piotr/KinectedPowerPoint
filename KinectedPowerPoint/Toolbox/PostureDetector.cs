using System;
using Microsoft.Kinect;

namespace KinectedPowerPoint.Toolbox
{
    public abstract class PostureDetector
    {
        public event Action<Posture> OnPostureDetected;

        private readonly int accumulatorTarget;
        private int accumulator;
        private Posture previousPosture = Posture.None;
        private Posture accumulatedPosture = Posture.None;

        public Posture CurrentPosture
        {
            get { return previousPosture; }
            protected set { previousPosture = value; }
        }

        protected PostureDetector(int accumulators)
        {
            accumulatorTarget = accumulators;
        }

        public abstract void TrackPostures(Skeleton skeleton);

        protected void RaisePostureDetected(Posture posture)
        {
            if (accumulator < accumulatorTarget)
            {
                if (accumulatedPosture != posture)
                {
                    accumulator = 0;
                    accumulatedPosture = posture;
                }
                accumulator++;
                return;
            }

            if (previousPosture == posture)
                return;

            previousPosture = posture;
            if (OnPostureDetected != null)
                OnPostureDetected(posture);

            accumulator = 0;
        }

        protected void Reset()
        {
            previousPosture = Posture.None;
            accumulator = 0;
        }
    }

    public enum Posture
    {
        None,
        GesturesOn,
        CatchCursor
    }
}
