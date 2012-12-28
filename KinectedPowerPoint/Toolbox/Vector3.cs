using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectedPowerPoint.Toolbox
{
    [Serializable]
    public class Vector3 
    {
        public float X;
        public float Y;
        public float Z;

        public static Vector3 Zero
        {
            get
            {
                return new Vector3(0, 0, 0);
            }
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector3 operator *(Vector3 left, float value)
        {
            return new Vector3(left.X * value, left.Y * value, left.Z * value);
        }
        public static Vector3 operator *(float value, Vector3 left)
        {
            return left * value;
        }
        public static Vector3 operator /(Vector3 left, float value)
        {
            return new Vector3(left.X / value, left.Y / value, left.Z / value);
        }
        public static Vector3 ToVector3(SkeletonPoint vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

    }
}
