#region License
// The MIT License (MIT)
// Copyright (c) 2013-2014 Hypnocube, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion
using System;

namespace Hypnocube.Demo.Model.Renderer
{
    namespace Math3D
    {
        /// <summary>
        ///     Represent a 3D vector
        /// </summary>
        public class Vector3D
        {
            public Vector3D(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public double this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return X;
                        case 1:
                            return Y;
                        case 2:
                            return Z;
                        case 3:
                            return 1.0;
                        default:
                            return 0;
                    }
                }
                set
                {
                    switch (i)
                    {
                        case 0:
                            X = value;
                            break;
                        case 1:
                            Y = value;
                            break;
                        case 2:
                            Z = value;
                            break;
                    }
                }
            }

            public static Vector3D operator +(Vector3D a, Vector3D b)
            {
                return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }

            public static Vector3D operator -(Vector3D a, Vector3D b)
            {
                return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            }

            public static Vector3D operator *(double a, Vector3D b)
            {
                return new Vector3D(a*b.X, a*b.Y, a*b.Z);
            }

            public static Vector3D Cross(Vector3D a, Vector3D b)
            {
                var x = a.Y*b.Z - a.Z*b.Y;
                var y = a.Z*b.X - a.X*b.Z;
                var z = a.X*b.Y - a.Y*b.X;
                return new Vector3D(x, y, z);
            }

            internal double Length()
            {
                return Math.Sqrt(X*X + Y*Y + Z*Z);
            }

            internal void Unit()
            {
                var d = Length();
                if (d != 0)
                {
                    X /= Length();
                    Y /= Length();
                    Z /= Length();
                }
            }
        }

        public class Matrix3D
        {
            public Matrix3D()
            {
                Values = new double[4, 4];
                for (var i = 0; i < 4; ++i)
                    Values[i, i] = 1; // identity matrix
            }

            public double[,] Values { get; set; }

            public double this[int i, int j]
            {
                get { return Values[i, j]; }
                set { Values[i, j] = value; }
            }

            public static Matrix3D operator *(Matrix3D a, Matrix3D b)
            {
                var m = new Matrix3D();
                for (var i = 0; i < 4; ++i)
                    for (var j = 0; j < 4; ++j)
                    {
                        var s = 0.0;
                        for (var k = 0; k < 4; ++k)
                            s += a[i, k]*b[k, j];
                        m[i, j] = s;
                    }
                return m;
            }

            public static Vector3D operator *(Matrix3D a, Vector3D b)
            {
                var v = new Vector3D(0, 0, 0);
                for (var i = 0; i < 4; ++i)
                    for (var j = 0; j < 4; ++j)
                        v[i] += a[i, j]*b[j];
                return v;
            }

            /// <summary>
            ///     Create a scaling matrix
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <returns></returns>
            public static Matrix3D Scale(double x, double y, double z)
            {
                var m = new Matrix3D();
                m[0, 0] = x;
                m[1, 1] = y;
                m[2, 2] = z;
                return m;
            }

            /// <summary>
            ///     Create a translation matrix
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            public static Matrix3D Translation(double x, double y, double z)
            {
                var m = new Matrix3D();
                m[0, 3] = x;
                m[1, 3] = y;
                m[2, 3] = z;
                return m;
            }


            /// <summary>
            ///     Create X rotation matrix with angle in radians
            /// </summary>
            /// <param name="angle"></param>
            /// <returns></returns>
            internal static Matrix3D XRotation(double angle)
            {
                var m = new Matrix3D();
                var c = Math.Cos(angle);
                var s = Math.Sin(angle);
                m[1, 1] = c;
                m[1, 2] = s;
                m[2, 1] = -s;
                m[2, 2] = c;
                return m;
            }

            /// <summary>
            ///     Create Y rotation matrix with angle in radians
            /// </summary>
            /// <param name="angle"></param>
            /// <returns></returns>
            internal static Matrix3D YRotation(double angle)
            {
                var m = new Matrix3D();
                var c = Math.Cos(angle);
                var s = Math.Sin(angle);
                m[0, 0] = c;
                m[0, 2] = s;
                m[2, 0] = -s;
                m[2, 2] = c;
                return m;
            }

            /// <summary>
            ///     Create Z rotation matrix with angle in radians
            /// </summary>
            /// <param name="angle"></param>
            /// <returns></returns>
            internal static Matrix3D ZRotation(double angle)
            {
                var m = new Matrix3D();
                var c = Math.Cos(angle);
                var s = Math.Sin(angle);
                m[0, 0] = c;
                m[0, 1] = s;
                m[1, 0] = -s;
                m[1, 1] = c;
                return m;
            }
        }
    }
}