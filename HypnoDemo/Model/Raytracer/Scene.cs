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
using System.Linq;
using Hypnocube.Demo.Model.Raytracer.Math3D;

namespace Hypnocube.Demo.Model.Raytracer
{
    internal class Scene
    {
        public List<Animation> Animations = new List<Animation>();
        public List<Primitive> Primitives = new List<Primitive>();

        public Scene()
        {
            //BasicScene();
            ComplexScene();
        }

        /// <summary>
        ///     Update scene over the time elapsed
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(double deltaTime)
        {
            foreach (var anim in Animations)
                anim.Update(deltaTime);
        }

        public void ComplexScene()
        {
            // ground plane
            Primitives.Add(new Plane(new Vector3D(0, 1, 0), new Vector3D(0, -2, 0)));
            Primitives.Last().Name = "plane";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Diffuse = 1.0;
            Primitives.Last().Material.Color.Set(0.3, 0.3, 0.8);

            // backstop
            //Primitives.Add(new Plane(new Vector3D(0, 0, -1), new Vector3D(0, 0, 15)));
            //Primitives.Last().Name = "plane";
            //Primitives.Last().Material.Reflection = 0.0;
            //Primitives.Last().Material.Diffuse = 1.0;
            //Primitives.Last().Material.Color.Set(0.3, 0.3, 0.5);


            // origin red sphere
            var p1 = new Sphere(new Vector3D(0, 0.0, 0), 1.0);
            Primitives.Add(p1);
            Primitives.Last().Name = "center sphere";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Diffuse = 1.0;
            Primitives.Last().Material.Specular = 1.0;
            Primitives.Last().Material.Color.Set(1.0, 0, 0);
            Animations.Add(new Animation(t => p1.Center.Y = 2*Math.Sin(t)));

            // green sphere
            var p2 = new Sphere(new Vector3D(-4, 0.0, 0), 1.0);
            Primitives.Add(p2);
            Primitives.Last().Name = "center sphere";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Diffuse = 1.0;
            Primitives.Last().Material.Specular = 1.0;
            Primitives.Last().Material.Color.Set(0.0, 1.0, 0);
            Animations.Add(new Animation(t => p2.Center.Y = 2*Math.Sin(t + 2)));

            // blue sphere
            var p3 = new Sphere(new Vector3D(4, 0.0, 0), 1.0);
            Primitives.Add(p3);
            Primitives.Last().Name = "center sphere";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Diffuse = 1.0;
            Primitives.Last().Material.Specular = 1.0;
            Primitives.Last().Material.Color.Set(0.0, 0.0, 1.0);
            Animations.Add(new Animation(t => p3.Center.Y = 2*Math.Sin(t - 2)));

            // yellow sphere
            var p4 = new Sphere(new Vector3D(8, 0.0, 0), 1.0);
            Primitives.Add(p4);
            Primitives.Last().Name = "center sphere";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Diffuse = 1.0;
            Primitives.Last().Material.Specular = 1.0;
            Primitives.Last().Material.Color.Set(0.0, 1.0, 1.0);
            Animations.Add(new Animation(t => p4.Center.Y = 2*Math.Sin(t*1.1 - 1.8)));

            // orange sphere
            var p5 = new Sphere(new Vector3D(-8, 0.0, 0), 1.0);
            Primitives.Add(p5);
            Primitives.Last().Name = "center sphere";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Diffuse = 1.0;
            Primitives.Last().Material.Specular = 1.0;
            Primitives.Last().Material.Color.Set(1.0, 1.0, 0.5);
            Animations.Add(new Animation(t => p5.Center.Y = 2*Math.Sin(t/1.1 + 1.5)));


#if false
    // glass sphere
            var p4 = new Sphere(new Vector3D(3, 0.0, -1), 2.0);
            Primitives.Add(p4);
            Primitives.Last().Name = "center sphere";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Diffuse = 0.0;
            Primitives.Last().Material.Specular = 1.0;
            Primitives.Last().Material.Refraction = 1.0;
            Primitives.Last().Material.RefractiveIndex = 2.0;
            Primitives.Last().Material.Color.Set(0.1, 0.1, 0.1);
            Animations.Add(new Animation(t =>
            {
                p4.Center.X = 2 * Math.Cos(t);
                p4.Center.Z = 2 * Math.Sin(t);
                p4.Center.Y = 1 * Math.Cos(3.4*t);

            }));
#endif

            // light source 
            Primitives.Add(new Sphere(new Vector3D(0, 15, -4), 0.01));
            Primitives.Last().Light = true;
            Primitives.Last().Material.Color.Set(1, 1, 1);
        }


        public void BasicScene()
        {
            // ground plane
            Primitives.Add(new Plane(new Vector3D(0, 1, 1), new Vector3D(0, 4, 0)));
            Primitives.Last().Name = "plane";
            Primitives.Last().Material.Reflection = 0;
            Primitives.Last().Material.Diffuse = 1.0;
            Primitives.Last().Material.Color.Set(0.4, 0.3, 0.3);

            // big sphere
            Primitives.Add(new Sphere(new Vector3D(1, -0.8, 3), 2.5));
            Primitives.Last().Name = "big sphere";
            Primitives.Last().Material.Reflection = 0.6;
            Primitives.Last().Material.Specular = 0.6;
            Primitives.Last().Material.Color.Set(0.7, 0.7, 0.7);
            // small sphere
            Primitives.Add(new Sphere(new Vector3D(-5.5, -0.5, 4), 2));
            Primitives.Last().Name = "small sphere";
            Primitives.Last().Material.Reflection = 1.0;
            Primitives.Last().Material.Specular = 0.6;
            Primitives.Last().Material.Diffuse = 0.1;
            Primitives.Last().Material.Color.Set(0.7, 0.7, 1.0);
            // light source 1
            Primitives.Add(new Sphere(new Vector3D(0, 5, 5), 0.1));
            Primitives.Last().Light = true;
            Primitives.Last().Material.Color.Set(0.6, 0.6, 0.6);
            // light source 2
            Primitives.Add(new Sphere(new Vector3D(2, 5, 1), 0.1));
            Primitives.Last().Light = true;
            Primitives.Last().Material.Color.Set(0.7, 0.7, 0.9);
        }
    }
}