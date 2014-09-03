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
    internal class Plane : Primitive
    {
        public Vector3D Normal;
        public Vector3D Point;

        public Plane(Vector3D normal, Vector3D pt)
        {
            Normal = normal;
            Point = pt;
        }

        public override IntersectionResult Intersect(Ray ray, ref double distance)
        {
            var denominator = Vector3D.Dot(ray.Direction, Normal);
            if (Math.Abs(denominator) > 0.001)
            {
                var numerator = Vector3D.Dot(Point - ray.Origin, Normal);
                var d = numerator/denominator;
                if (d > 0)
                {
                    if (d < distance)
                    {
                        distance = d;
                        return IntersectionResult.Hit;
                    }
                }
            }

            return IntersectionResult.Miss;
        }

        public override Vector3D GetNormal(Vector3D point)
        {
            return Normal;
        }
    }
}