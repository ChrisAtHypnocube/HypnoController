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
using Hypnocube.Demo.Model.Renderer;
using Hypnocube.Demo.Model.Renderer.Math3D;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Polyhedra : DemoBase
    {
        private readonly int[] colors =
        {
            0, 255, 255,
            255, 255, 0,
            255, 0, 0,
            0, 255, 0,
            0, 0, 255
        };

        private readonly Render renderer = new Render();

        private double angle;

        public Polyhedra(int w, int h)
            : base(w, h)
        {
        }

        public override void Update()
        {
            base.Update();
            Fill(0, 0, 0);

            angle += 0.06;

            var meshes = new[]
            {
                Renderer.Polyhedra.Tetrahedron(),
                Renderer.Polyhedra.Octahedron(),
                Renderer.Polyhedra.Dodecahedron(),
                Renderer.Polyhedra.Icosahedron(),
                Renderer.Polyhedra.Cube()
            };

            const int max = 5;
            var hue = (Frame & 511)/511.0;
            double r1, g1, b1;
            HslToRgb(hue, 1, 0.5, out r1, out g1, out b1);
            var zero = new Vector3D(0, 0, 0);
            for (var index = 0; index < max; ++index)
            {
                var center = index - (max - 1)/2.0;
                var mesh = meshes[index];

                var scale = Math.Min(Width, Height)*0.35*(Math.Sin(angle + index*Math.PI*2/max) + 2)/3;

                var dx = (1.0/max*index + 1.0/max/2.0)*Width;
                var dy = Height/2;

                var mat =
                    Matrix3D.Translation(dx, dy, 0)*
                    Matrix3D.Scale(scale, scale, scale)*
                    Matrix3D.XRotation(Math.PI*Math.Sin(angle + center/3.0))*Matrix3D.YRotation(angle/2 - center/4.0)*
                    Matrix3D.ZRotation(angle*1.2 + 1);

                Action<int, int, int> facePixel = (i, j, f) =>
                {
                    // get normal
                    var ind = mesh.Indices[f];
                    var p0 = mesh.Points[ind[0]];
                    var p1 = mesh.Points[ind[1]];
                    var p2 = mesh.Points[ind[2]];
                    var normal = Vector3D.Cross(p0 - p1, p2 - p1);
                    normal.Unit();

                    // now have base hue. Shade as normal faces away from Z
                    var disp = mat*normal - mat*zero;
                    disp.Unit();

                    var shade = disp.Z;
                    if (shade < 0) shade = 0;
                    if (shade > 1) shade = 1;

                    // Debug.WriteLine(shade);

                    shade = Math.Sin(shade*Math.PI/2);

                    var r = (int) (colors[index*3]*shade);
                    var g = (int) (colors[index*3 + 1]*shade);
                    var b = (int) (colors[index*3 + 2]*shade);


                    SetPixel(i, j, r, g, b);
                };

                renderer.Draw1(Width, Height, mat, mesh, facePixel,
                    (i, j, edge) => SetPixel(i, j, 255, 255, 255)
                    );
            }
        }
    }
}