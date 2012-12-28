using System;
using Microsoft.Kinect;


namespace KinectedPowerPoint.Toolbox
{
    class InitiatePostureDetector : PostureDetector
    {
        public float MaxRange { get; set; }

        public InitiatePostureDetector() : base(30)
        {
            MaxRange = 0.25f;
        }

        public override void TrackPostures(Skeleton skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3 headPosition = null;
            Vector3 leftHandPosition = null;
            Vector3 rightHandPosition = null;

            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.JointType)
                {
                    case JointType.Head:
                        headPosition = joint.Position.ToVector3();
                        break;
                    case JointType.HandLeft:
                        leftHandPosition = joint.Position.ToVector3();
                        break;
                    case JointType.HandRight:
                        rightHandPosition = joint.Position.ToVector3();
                        break;
                }
            }

            // Right hand up - Gesture on
            if (CheckHandUpPosture(headPosition, rightHandPosition))
            {
                RaisePostureDetected(Posture.GesturesOn);
                return;
            }

            // Left hand up - Catch cursor
            if (CheckHandUpPosture(headPosition, leftHandPosition))
            {
                RaisePostureDetected(Posture.CatchCursor);
                return;
            }

            Reset();
        }

        bool CheckHandUpPosture(Vector3 headPosition, Vector3 handPosition)
        {
            if (handPosition == null || headPosition == null)
                return false;

            if (Math.Abs(handPosition.X - headPosition.X) < MaxRange)
                return false;

            if (Math.Abs(handPosition.Y - headPosition.Y) > MaxRange)
                return false;

            if (Math.Abs(handPosition.Z - headPosition.Z) > MaxRange)
                return false;

            return true;
        }

    }
}
