using System.Drawing;
using SharpDX;
using System;
using ScriptKidAntiCheat.Internal.Raw;

/*
 * Credit: https://github.com/rciworks/RCi.Tutorials.Csgo.Cheat.External
 */
namespace ScriptKidAntiCheat.Utils.Maths
{
    public static class GfxMath
    {

        private const double _180_Over_PI = 180.0 / Math.PI;

        public static float RadianToDegree(this float radian)
        {
            return (float)(radian * _180_Over_PI);
        }

        public static float AngleToSigned(this Vector3 vector, Vector3 other, Vector3 about)
        {
            // validate
            if (vector.IsParallelTo(about, 1E-9f))
            {
                throw new ArgumentException($"'{nameof(vector)}' is parallel to '{nameof(about)}'.");
            }
            if (other.IsParallelTo(about, 1E-9f))
            {
                throw new ArgumentException($"'{nameof(other)}' is parallel to '{nameof(about)}'.");
            }

            // project vectors on a plane
            var plane = new Plane3D(about, new Vector3());
            var vectorOnPlane = plane.ProjectVector(vector).vector.Normalized();
            var otherOnPlane = plane.ProjectVector(other).vector.Normalized();

            // get angle
            var sign = vectorOnPlane.Cross(otherOnPlane).Normalized().Dot(plane.Normal);
            return AngleBetweenUnitVectors(vectorOnPlane, otherOnPlane) * sign;
        }

        public static float AngleBetweenUnitVectors(Vector3 leftNormalized, Vector3 rightNormalized)
        {
            return AcosClamped(leftNormalized.Dot(rightNormalized));
        }

        public static float AcosClamped(float value, float tolerance = 1E-6f)
        {
            if (value > 1 - tolerance)
            {
                return 0;
            }
            if (value < tolerance - 1)
            {
                return (float)System.Math.PI;
            }
            return (float)System.Math.Acos(value);
        }

        /// <summary>
        /// Angle between vectors.
        /// </summary>
        public static float AngleTo(this Vector3 vector, Vector3 other)
        {
            return (float)System.Math.Acos(vector.Normalized().Dot(other.Normalized()));
        }

        /// <inheritdoc cref="Vector3.Cross"/>
        public static Vector3 Cross(this Vector3 left, Vector3 right)
        {
            return Vector3.Cross(left, right);
        }

        /// <inheritdoc cref="Vector3.Dot"/>
        public static float Dot(this Vector3 left, Vector3 right)
        {
            return Vector3.Dot(left, right);
        }

        /// <summary>
        /// Get matrix from given axis and origin.
        /// </summary>
        public static Matrix GetMatrix(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, Vector3 origin)
        {
            return new Matrix
            {
                M11 = xAxis.X,
                M12 = xAxis.Y,
                M13 = xAxis.Z,

                M21 = yAxis.X,
                M22 = yAxis.Y,
                M23 = yAxis.Z,

                M31 = zAxis.X,
                M32 = zAxis.Y,
                M33 = zAxis.Z,

                M41 = origin.X,
                M42 = origin.Y,
                M43 = origin.Z,
                M44 = 1,
            };
        }

        /// <summary>
        /// Get orthogonal axis from given normal.
        /// </summary>
        public static void GetOrthogonalAxis(Vector3 normal, out Vector3 xAxis, out Vector3 yAxis, out Vector3 zAxis)
        {
            zAxis = normal.Normalized();
            var zAxisWorld = new Vector3(0, 0, 1);
            var angleToAxisZ = zAxis.AngleTo(zAxisWorld);
            if (angleToAxisZ < System.Math.PI * 0.25 || angleToAxisZ > System.Math.PI * 0.75)
            {
                // too close to z-axis, use y-axis
                xAxis = new Vector3(0, 1, 0).Cross(zAxis).Normalized();
            }
            else
            {
                // use z-axis
                xAxis = zAxis.Cross(zAxisWorld).Normalized();
            }
            yAxis = zAxis.Cross(xAxis).Normalized();
        }

        public static Vector3 GetVectorFromEulerAngles(double phi, double theta)
        {
            return new Vector3
            (
                (float)(System.Math.Cos(phi) * System.Math.Cos(theta)),
                (float)(System.Math.Cos(phi) * System.Math.Sin(theta)),
                (float)-System.Math.Sin(phi)
            ).Normalized();
        }

        /// <summary>
        /// Is vector parallel to other vector?
        /// </summary>
        public static bool IsParallelTo(this Vector3 vector, Vector3 other, float tolerance = 1E-6f)
        {
            return System.Math.Abs(1.0 - System.Math.Abs(vector.Normalized().Dot(other.Normalized()))) <= tolerance;
        }

        /// <inheritdoc cref="Vector3.Normalize(Vector3)"/>
        public static Vector3 Normalized(this Vector3 value)
        {
            return Vector3.Normalize(value);
        }

        /// <summary>
        /// Convert to matrix 4x4.
        /// </summary>
        public static Matrix ToMatrix(this in matrix3x4_t matrix)
        {
            return new Matrix
            {
                M11 = matrix.m00,
                M12 = matrix.m01,
                M13 = matrix.m02,

                M21 = matrix.m10,
                M22 = matrix.m11,
                M23 = matrix.m12,

                M31 = matrix.m20,
                M32 = matrix.m21,
                M33 = matrix.m22,

                M41 = matrix.m30,
                M42 = matrix.m31,
                M43 = matrix.m32,
                M44 = 1,
            };
        }

        /// <summary>
        /// Transform value.
        /// </summary>
        public static Vector3 Transform(this in Matrix matrix, Vector3 value)
        {
            var wInv = 1.0 / ((double)matrix.M14 * (double)value.X + (double)matrix.M24 * (double)value.Y + (double)matrix.M34 * (double)value.Z + (double)matrix.M44);
            return new Vector3
            (
                (float)(((double)matrix.M11 * (double)value.X + (double)matrix.M21 * (double)value.Y + (double)matrix.M31 * (double)value.Z + (double)matrix.M41) * wInv),
                (float)(((double)matrix.M12 * (double)value.X + (double)matrix.M22 * (double)value.Y + (double)matrix.M32 * (double)value.Z + (double)matrix.M42) * wInv),
                (float)(((double)matrix.M13 * (double)value.X + (double)matrix.M23 * (double)value.Y + (double)matrix.M33 * (double)value.Z + (double)matrix.M43) * wInv)
            );
        }

        private const double _PI_Over_180 = Math.PI / 180.0;
        public static double DegreeToRadian(this double degree)
        {
            return (double)(degree * _PI_Over_180);
        }
    }

}