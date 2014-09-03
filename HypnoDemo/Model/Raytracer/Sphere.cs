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
using Hypnocube.Demo.Model.Raytracer.Math3D;

namespace Hypnocube.Demo.Model.Raytracer
{
    internal class Sphere : Primitive
    {
        public Sphere(Vector3D center, double radius)
        {
            Center = center;
            Radius = radius;
            RadiusSquared = radius*radius;
        }

        public Vector3D Center { get; set; }
        public double Radius { get; private set; }

        public double RadiusSquared { get; private set; }

        public override IntersectionResult Intersect(Ray ray, ref double distance)
        {
            var v = ray.Origin - Center;
            var b = -Vector3D.Dot(v, ray.Direction);
            var det = (b*b) - Vector3D.Dot(v, v) + RadiusSquared;
            var retval = IntersectionResult.Miss;
            if (det > 0)
            {
                det = Math.Sqrt(det);
                var i1 = b - det;
                var i2 = b + det;
                if (i2 > 0)
                {
                    if (i1 < 0)
                    {
                        if (i2 < distance)
                        {
                            distance = i2;
                            retval = IntersectionResult.InPrimitive;
                        }
                    }
                    else
                    {
                        if (i1 < distance)
                        {
                            distance = i1;
                            retval = IntersectionResult.Hit;
                        }
                    }
                }
            }
            return retval;
        }

        public override Vector3D GetNormal(Vector3D point)
        {
            var normal = point - Center;
            normal.Normalize();
            return normal;
        }
    }
}