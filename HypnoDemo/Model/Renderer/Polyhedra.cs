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
using System.Collections.Generic;
using Hypnocube.Demo.Model.Renderer.Math3D;

namespace Hypnocube.Demo.Model.Renderer
{
    internal class Polyhedra
    {
        /// <summary>
        ///     Return a unit Icosahedron
        /// </summary>
        /// <returns></returns>
        public static Mesh Icosahedron()
        {
            // PolyhedronData["Icosahedron", "Faces"] // N

            var coords = new[]
            {
                0.0, 0.0, -0.951057, 0.0, 0.0, 0.951057, -0.850651,
                0.0, -0.425325, 0.850651, 0.0,
                0.425325, 0.688191, -0.5, -0.425325, 0.688191,
                0.5, -0.425325, -0.688191, -0.5, 0.425325, -0.688191, 0.5,
                0.425325, -0.262866, -0.809017, -0.425325, -0.262866,
                0.809017, -0.425325, 0.262866, -0.809017, 0.425325, 0.262866,
                0.809017, 0.425325
            };

            var pts = new List<Vector3D>();
            for (var i = 0; i < coords.Length; i += 3)
                pts.Add(new Vector3D(coords[i], coords[i + 1], coords[i + 2]));


            var faces = new[]
            {
                2, 12, 8, 2, 8, 7, 2, 7, 11, 2, 11, 4, 2, 4,
                12, 5, 9, 1, 6, 5, 1, 10, 6, 1, 3, 10, 1, 9, 3, 1, 12,
                10, 8, 8, 3, 7, 7, 9, 11, 11, 5, 4, 4, 6, 12, 5, 11,
                9, 6, 4, 5, 10, 12, 6, 3, 8, 10, 9, 7, 3
            };

            var indices = new List<List<int>>();
            for (var i = 0; i < faces.Length; i += 3)
            {
                var f = new List<int>();
                f.Add(faces[i + 2] - 1);
                f.Add(faces[i + 1] - 1);
                f.Add(faces[i + 0] - 1);
                indices.Add(f);
            }

            var colors = new List<Color>
            {
                new Color(0, 0, 255),
                new Color(0, 255, 0),
                new Color(255, 0, 255),
                new Color(255, 255, 0),
                new Color(128, 128, 128),
                new Color(0, 255, 255),
                new Color(255, 0, 0),
                new Color(255, 0, 255),
                new Color(255, 255, 0),
                new Color(128, 128, 128),
                new Color(255, 128, 0),
                new Color(0, 255, 128),
                new Color(255, 255, 0),
                new Color(128, 128, 128),
                new Color(0, 255, 255),
                new Color(255, 0, 0),
                new Color(255, 0, 255),
                new Color(255, 255, 0),
                new Color(128, 128, 128),
                new Color(255, 128, 0)
            };
            return new Mesh(pts, indices, colors);
        }

        /// <summary>
        ///     Return a unit Dodecahedron containing [-1,-1,-1] to [1,1,1]
        /// </summary>
        /// <returns></returns>
        public static Mesh Dodecahedron()
        {
            // points (±1, ±1, ±1)(0, ±1/φ, ±φ)(±1/φ, ±φ, 0)(±φ, 0, ±1/φ)
            // φ =  (1 + √5) / 2 golden mean
            // edge length 2/φ = √5 – 1


            // PolyhedronData["Dodecahedron", "Faces"] // N

            var coords = new[]
            {
                -1.37638, 0.0, 0.262866, 1.37638, 0.0, -0.262866, -0.425325, -1.30902, 0.262866, -0.425325,
                1.30902, 0.262866, 1.11352, -0.809017, 0.262866, 1.11352,
                0.809017, 0.262866, -0.262866, -0.809017, 1.11352, -0.262866,
                0.809017, 1.11352, -0.688191, -0.5, -1.11352, -0.688191,
                0.5, -1.11352, 0.688191, -0.5, 1.11352, 0.688191, 0.5,
                1.11352, 0.850651,
                0.0, -1.11352, -1.11352, -0.809017, -0.262866, -1.11352,
                0.809017, -0.262866, -0.850651, 0.0,
                1.11352, 0.262866, -0.809017, -1.11352, 0.262866,
                0.809017, -1.11352, 0.425325, -1.30902, -0.262866, 0.425325,
                1.30902, -0.262866
            };

            var pts = new List<Vector3D>();
            for (var i = 0; i < coords.Length; i += 3)
                pts.Add(new Vector3D(coords[i], coords[i + 1], coords[i + 2]));


            var faces = new[]
            {
                15, 10, 9, 14, 1, 2, 6, 12, 11, 5, 5, 11, 7, 3,
                19, 11, 12, 8, 16, 7, 12, 6, 20, 4, 8, 6, 2, 13, 18,
                20, 2, 5, 19, 17, 13, 4, 20, 18, 10, 15, 18, 13, 17, 9,
                10, 17, 19, 3, 14, 9, 3, 7, 16, 1, 14, 16, 8, 4, 15, 1
            };

            var indices = new List<List<int>>();
            for (var i = 0; i < faces.Length; i += 5)
            {
                var f = new List<int>();
                f.Add(faces[i + 4] - 1);
                f.Add(faces[i + 3] - 1);
                f.Add(faces[i + 2] - 1);
                f.Add(faces[i + 1] - 1);
                f.Add(faces[i + 0] - 1);
                indices.Add(f);
            }

            var colors = new List<Color>
            {
                new Color(0, 0, 255),
                new Color(0, 255, 0),
                new Color(255, 0, 255),
                new Color(255, 255, 0),
                new Color(128, 128, 128),
                new Color(0, 255, 255),
                new Color(255, 0, 0),
                new Color(255, 0, 255),
                new Color(255, 255, 0),
                new Color(128, 128, 128),
                new Color(255, 128, 0),
                new Color(0, 255, 128)
            };
            return new Mesh(pts, indices, colors);
        }


        /// <summary>
        ///     Return a unit octahedron in [-1,-1,-1] to [1,1,1]
        /// </summary>
        /// <returns></returns>
        public static Mesh Octahedron()
        {
            const double s = 1;
            var pts = new List<Vector3D>
            {
                new Vector3D(s, 0, 0),
                new Vector3D(0, s, 0),
                new Vector3D(-s, 0, 0),
                new Vector3D(0, -s, 0),
                new Vector3D(0, 0, s),
                new Vector3D(0, 0, -s)
            };

            var indices = new List<List<int>>
            {
                new List<int> {0, 1, 4},
                new List<int> {1, 2, 4},
                new List<int> {2, 3, 4},
                new List<int> {3, 0, 4},
                new List<int> {1, 0, 5},
                new List<int> {2, 1, 5},
                new List<int> {3, 2, 5},
                new List<int> {0, 3, 5}
            };
            var colors = new List<Color>
            {
                new Color(0, 0, 255),
                new Color(0, 255, 0),
                new Color(0, 255, 255),
                new Color(255, 0, 0),
                new Color(255, 0, 255),
                new Color(255, 255, 0),
                new Color(128, 128, 128),
                new Color(255, 128, 0),
                new Color(0, 255, 128)
            };
            return new Mesh(pts, indices, colors);
        }

        /// <summary>
        ///     Return a unit tetrahedron in [-1,-1,-1] to [1,1,1]
        /// </summary>
        /// <returns></returns>
        public static Mesh Tetrahedron()
        {
            const double s = 1;
            var pts = new List<Vector3D>
            {
                new Vector3D(s, s, s),
                new Vector3D(-s, -s, s),
                new Vector3D(-s, s, -s),
                new Vector3D(s, -s, -s),
            };

            var indices = new List<List<int>>
            {
                new List<int> {0, 2, 1},
                new List<int> {0, 1, 3},
                new List<int> {0, 3, 2},
                new List<int> {1, 2, 3}
            };
            var colors = new List<Color>
            {
                new Color(255, 0, 0),
                new Color(0, 255, 0),
                new Color(0, 0, 255),
                new Color(128, 128, 128)
            };
            return new Mesh(pts, indices, colors);
        }

        /// <summary>
        ///     Return a unit cube in [-1,-1,-1] to [1,1,1]
        /// </summary>
        /// <returns></returns>
        public static Mesh Cube()
        {
            const double s = 1;
            var pts = new List<Vector3D>
            {
                new Vector3D(s, s, s),
                new Vector3D(-s, s, s),
                new Vector3D(-s, -s, s),
                new Vector3D(s, -s, s),
                new Vector3D(s, s, -s),
                new Vector3D(s, -s, -s),
                new Vector3D(-s, -s, -s),
                new Vector3D(-s, s, -s),
            };

            var indices = new List<List<int>>
            {
                new List<int> {0, 1, 2, 3},
                new List<int> {4, 5, 6, 7},
                new List<int> {0, 4, 7, 1},
                new List<int> {1, 7, 6, 2},
                new List<int> {2, 6, 5, 3},
                new List<int> {3, 5, 4, 0}
            };
            var colors = new List<Color>
            {
                new Color(255, 0, 0),
                new Color(0, 255, 0),
                new Color(0, 0, 255),
                new Color(255, 128, 0),
                new Color(0, 255, 128),
                new Color(128, 0, 255)
            };
            return new Mesh(pts, indices, colors);
        }
    }
}