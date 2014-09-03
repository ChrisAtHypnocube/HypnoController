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
using Hypnocube.Demo.Model.Renderer.Math3D;

namespace Hypnocube.Demo.Model.Renderer
{
    internal class Render
    {
        public void Draw1(int w, int h, Matrix3D projection,
            Mesh mesh,
            Action<int, int, int> setFacePixel,
            Action<int, int, int> setEdgePixel
            )
        {
            for (var fi = 0; fi < mesh.Indices.Count; ++fi)
            {
                var face = mesh.Indices[fi];

                if (face.Count < 3) continue; // nothing to draw
                // check normal
                var v0 = projection*mesh.Points[face[0]];
                var v1 = projection*mesh.Points[face[1]];
                var v2 = projection*mesh.Points[face[2]];
                if (Vector3D.Cross(v0 - v1, v2 - v1).Z < 0)
                    continue;

                // projected points
                var ppts = new List<Vector3D>();
                for (var i = 0; i < face.Count; ++i)
                    ppts.Add(projection*mesh.Points[face[i]]);
                ppts.Add(ppts[0]); // double up on last one

                // draw filled polygon
                // get min, max y
                int minY = int.MaxValue, maxY = int.MinValue;
                foreach (var pt in ppts)
                {
                    minY = Math.Min(minY, (int) pt.Y);
                    maxY = Math.Max(maxY, (int) pt.Y);
                }
                // space for left, right ends
                var left = new int[maxY - minY + 1];
                var right = new int[maxY - minY + 1];
                // initialize them
                for (var i = 0; i < left.Length; ++i)
                {
                    left[i] = int.MaxValue;
                    right[i] = int.MinValue;
                }
                // now walk all edges, setting left and right at each y value
                for (var i = 0; i < ppts.Count - 1; ++i)
                {
                    var p1 = ppts[i];
                    var p2 = ppts[i + 1];
                    int x1 = (int) p1.X, y1 = (int) p1.Y;
                    int x2 = (int) p2.X, y2 = (int) p2.Y;
                    DDA.Compute(x1, y1, x2, y2, (x, y) =>
                    {
                        left[y - minY] = Math.Min(left[y - minY], x);
                        right[y - minY] = Math.Max(right[y - minY], x);
                    }
                        );
                }
                // now walk and fill
                var color = mesh.FaceColors[fi];
                for (var j = 0; j < left.Length; ++j)
                {
                    for (var i = left[j]; i <= right[j]; ++i)
                        setFacePixel(i, j + minY, fi);
                }


                // draw wireframe
                for (var i = 0; i < ppts.Count - 1; ++i)
                {
                    var p1 = ppts[i];
                    var p2 = ppts[i + 1];
                    int x1 = (int) p1.X, y1 = (int) p1.Y;
                    int x2 = (int) p2.X, y2 = (int) p2.Y;
                    Draw.Line(x1, y1, x2, y2, (i1, j1) => setEdgePixel(i1, j1, fi));
                }
            }
        }
    }
}