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
    public enum IntersectionResult
    {
        Hit,
        Miss,
        InPrimitive
    }

    internal class Render
    {
        private const int MaxRayDepth = 4;

        private const double Epsilon = 0.0000001;
        private Scene scene;

        /// <summary>
        ///     Simple raytracing: intersect ray with every primitive to find color
        /// </summary>
        /// <returns></returns>
        private Primitive Raytrace(Ray ray, ref Color accumulatedColor, int recurseDepth, double defaultRefractiveIndex, ref double hitDepth)
        {
            if (recurseDepth > MaxRayDepth) 
                return null;
            // trace primary ray
            hitDepth = 1000000.0;
            Primitive prim = null;
            var result = IntersectionResult.Miss;
            // find the nearest intersection
            foreach (var pr in scene.Primitives)
            {
                var res = pr.Intersect(ray, ref hitDepth);
                if (res == IntersectionResult.Miss) 
                    continue;
                prim = pr;
                result = res; // 0 = miss, 1 = hit, -1 = hit from inside primitive
            }
            // no hit, terminate ray
            if (prim == null) 
                return null;
            // handle intersection
            if (prim.Light)
            {
                // we hit a light, stop tracing
                accumulatedColor = new Color(1, 1, 1);
            }
            else
            {
                // determine color at point of intersection
                var intersectionPoint = ray.Origin + ray.Direction*hitDepth;
                // trace lights
                foreach (var p in scene.Primitives)
                {
                    if (p.Light)
                    {
                        var light = p;
                        // calculate diffuse shading
                        var lightDirection = (light as Sphere).Center - intersectionPoint;
                        lightDirection.Normalize();
                        var normal = prim.GetNormal(intersectionPoint);

                        // handle point light source shadows
                        const double shade = 1.0;
#if false
                        if (light is Sphere)
                        {
                            Vector3D L2 = (light as Sphere).Center - intersectionPoint;
                            var tdist = L2.Length();
                            L2 *= (1.0f/tdist);
                            Ray r = new Ray(intersectionPoint + L2 * Epsilon, lightDirection);
                            foreach (var pr in scene.Primitives)
                                //for (int s = 0; s < scene->GetNrPrimitives(); s++)
                            {
                                //  Primitive* pr = scene->GetPrimitive(s);
                                if ((pr != light) && (pr.Intersect(r, ref tdist) != IntersectionResult.Miss))
                                {
                                    shade = 0;
                                    break;
                                }
                            }
                        }
#endif

                        if (prim.Material.Diffuse > 0)
                        {
                            var dot = Vector3D.Dot(normal, lightDirection);
                            if (dot > 0)
                            {
                                var diff = dot*shade*prim.Material.Diffuse;
                                // add diffuse component to ray color
                                accumulatedColor.Add(diff*prim.Material.Color*light.Material.Color);

                                // add some specular
                                var V = ray.Direction;
                                var R = lightDirection - 2.0*Vector3D.Dot(lightDirection, normal)*normal;
                                dot = Vector3D.Dot(V, R);
                                if (dot > 0)
                                {
                                    var spec = Math.Pow(dot, 20)*prim.Material.Specular*shade;
                                    // add specular component to ray color
                                    accumulatedColor.Add(spec*light.Material.Color);
                                }
                            }
                        }
                    }
                }
#if true
                // calculate reflection
                var refl = prim.Material.Reflection;
                if (refl > 0.0)
                {
                    var N = prim.GetNormal(intersectionPoint);
                    var R = ray.Direction - 2.0*Vector3D.Dot(ray.Direction, N)*N;
                    if (recurseDepth < MaxRayDepth)
                    {
                        var rcol = new Color(0, 0, 0);
                        var dist = double.MaxValue;
                        Raytrace(new Ray(intersectionPoint + R*Epsilon, R), ref rcol, recurseDepth + 1, defaultRefractiveIndex, ref dist);
                        accumulatedColor.Add(refl*rcol*prim.Material.Color);
                    }
                }

                // calculate refraction
                var refr = prim.Material.Refraction;
                if ((refr > 0) && (recurseDepth < MaxRayDepth))
                {
                    var rindex = prim.Material.RefractiveIndex;
                    var n = defaultRefractiveIndex/rindex;
                    var N = prim.GetNormal(intersectionPoint)*(float) result;
                    var cosI = -Vector3D.Dot(N, ray.Direction);
                    var cosT2 = 1.0f - n*n*(1.0f - cosI*cosI);
                    if (cosT2 > 0.0f)
                    {
                        var T = (n*ray.Direction + (n*cosI - Math.Sqrt(cosT2))*N);
                        var rcol = new Color(0, 0, 0);
                        var dist = Double.MaxValue;
                        Raytrace(new Ray(intersectionPoint + T*Epsilon, T), ref rcol, recurseDepth + 1, rindex, ref dist);
                        accumulatedColor.Add(rcol);
                    }
                }
#endif
            }
            // return pointer to primitive hit by primary ray
            return prim;
        }

        public void Draw(Scene scene, int width, int height, Action<int, int, int, int, int> DrawColor, Camera camera)
        {
            this.scene = scene;

            for (var i = 0; i < width; ++i)
                for (var j = 0; j < height; ++j)
                {
                    // negate y direction since image is upside down from world coords
                    //var dir = new Vector3D((double) (i - width/2.0)/maxd, -(double) (j - height/2.0)/maxd, 0) - origin;
                    //var ray = new Ray(origin,dir);

                    var ax = (i - width/2.0)/Math.Min(width, height);
                    var ay = (j - height/2.0)/Math.Min(width, height);
                    var ray = camera.ComputeCameraRay(ax, ay);

                    var hitColor = new Color(0, 0, 0);
                    var distance = double.MaxValue;
                    
                    Raytrace(ray, ref hitColor, 1, 1.0, ref distance);

                    // convert to pixel colors and set them
                    var red = (int) (hitColor.Red*256);
                    var green = (int) (hitColor.Green*256);
                    var blue = (int) (hitColor.Blue*256);
                    if (red > 255) red = 255;
                    if (green > 255) green = 255;
                    if (blue > 255) blue = 255;
                    DrawColor(i, j, red, green, blue);
                }
        }
    }
}