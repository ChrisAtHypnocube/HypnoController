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
using Hypnocube.Demo.Model.Raytracer;
using Hypnocube.Demo.Model.Raytracer.Math3D;

namespace Hypnocube.Demo.Model.Demos
{
    internal class RaytracerDemo : DemoBase
    {
        private readonly Camera camera;
        private readonly Render renderer = new Render();
        private readonly Scene scene = new Scene();

        public RaytracerDemo(int w, int h)
            : base(w, h)
        {
            camera = new Camera(
                new Vector3D(0, 0, -80), // eye
                new Vector3D(0, -0.5, 0), // aimpoint
                new Vector3D(0, -1, 0) // up direction
                );
            camera.ImageScale = 0.87;
        }

        public override void Update()
        {
            base.Update();
            renderer.Draw(scene, Width, Height,
                (i, j, r, g, b) => SetPixel(i, j, r, g, b),
                camera
                );
            scene.Update(1/10.0);
        }
    }
}