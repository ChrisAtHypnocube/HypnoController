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
using System.Collections.Generic;
using Hypnocube.Demo.Model.Raytracer.Math3D;
using Hypnocube.Demo.Model.Renderer;

namespace Hypnocube.Demo.Model.Demos
{
    internal class VectorBalls : DemoBase
    {
        public VectorBalls(int w, int h) : base(w, h)
        {
        }

/* vector ball ideas
 * 1. Tetra - edges are balls, colors change, different sizes on corners
 * 2. Cube, stretching and scaling
 * 3. Animated surfaces, ruled surfaces
 * 4. 3D lissajous animations
 * 5. Blends between objects
 * 6. disk, opening and closing as a bowl
 * 7. Helix
 * 8. Octopus like creature with tentacles waving around
 * 9. words and letters, scrolling on a zoomer path
 * 10. Circles
 * */
        // make nicer looking like here http://www.youtube.com/watch?v=VkaVDczP8c4
        // also see here http://www.youtube.com/watch?v=Be5HvIG5lYo


        private static void AddBalls(List<Ball> balls, Model model, double rx, double ry, double rz, double tx,
            double ty,
            double tz)
        {
            var transform =
                Matrix3D.Translation(tx, ty, tz)*
                Matrix3D.XRotation(rx)*Matrix3D.YRotation(ry)*Matrix3D.ZRotation(rz);
            foreach (var b in model.Balls)
            {
                balls.Add(new Ball(transform*b.Point, b.Radius, b.Color));
            }
        }

        public override void Update()
        {
            base.Update();
            Fill(0, 0, 0);
            var angle = Frame/50.0;

            var minSize = Math.Min(Height, Width);
            var c1 = new Cube(4, minSize*0.5);
            var c2 = new Circle(16, minSize/14.0, minSize*0.3);
            var c3 = new Lissajous(50, minSize/15.0, minSize*0.25);

            var balls = new List<Ball>();

            var maxSize = Math.Max(Height, Width);

            AddBalls(balls, c2, angle, angle/3, angle/2, -maxSize*0.3, 0, 0);
            AddBalls(balls, c3, angle, angle/2, angle/4, 0, 0, 0);
            AddBalls(balls, c1, angle, angle/4, angle/3, maxSize*0.3, 0, 0);

            DrawVectorBalls(balls);
        }

        #region Nested type: Circle

        private class Circle : Model
        {
            public Circle(int count, double ballRadius, double circleRadius)
            {
                for (var k = 0; k < count; ++k)
                {
                    var angle = (double) k/count*2*Math.PI;
                    var x = Math.Cos(angle)*circleRadius;
                    var y = Math.Sin(angle)*circleRadius;
                    var z = 0;
                    int r, g, b;
                    HslToRgb(angle/2.0/Math.PI, 1.0, 0.5, out r, out g, out b);
                    Balls.Add(new Ball(new Vector3D(x, y, z), ballRadius, new Color((byte) r, (byte) g, (byte) b)));
                }
            }
        }

        #endregion

        #region Nested type: Cube

        private class Cube : Model
        {
            public Cube(int side, double scale)
            {
                var radius = scale/2.0/side;
                for (var i = 0; i < side; ++i)
                    for (var j = 0; j < side; ++j)
                        for (var k = 0; k < side; ++k)
                        {
                            var x = scale*i/side - scale/2 + radius;
                            var y = scale*j/side - scale/2 + radius;
                            var z = scale*k/side - scale/2 + radius;
                            Balls.Add(new Ball(new Vector3D(x, y, z), radius, new Color(255, 0, 0)));
                        }
            }
        }

        #endregion

        #region Nested type: Lissajous

        /// <summary>
        ///     Lissajous 3D curve
        /// </summary>
        private class Lissajous : Model
        {
            private readonly double a;
            private readonly double b;
            private readonly double c;
            private readonly double d1;
            private readonly double d2;
            private readonly double d3;

            public Lissajous(int count, double ballRadius, double size)
            {
                a = 2;
                b = 3;
                c = 5;
                d1 = 0;
                d2 = 0;
                d3 = 0;
                for (var i = 0; i < count; ++i)
                {
                    var angle = i*Math.PI*2/count;
                    int r, g, bl;
                    HslToRgb(angle/2.0/Math.PI, 1.0, 0.5, out r, out g, out bl);
                    Balls.Add(new Ball(size*Point(angle), ballRadius, new Color((byte) r, (byte) g, (byte) bl)));
                }
            }

            private Vector3D Point(double angle)
            {
                return new Vector3D(Math.Sin(a*angle + d1), Math.Sin(b*angle + d2), Math.Sin(c*angle + d3));
            }
        }

        #endregion

        #region Nested type: Model

        private class Model
        {
            public readonly List<Ball> Balls = new List<Ball>();
        }

        #endregion
    }
}