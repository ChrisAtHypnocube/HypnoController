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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Annotations;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Mandelzoom : DemoBase
    {
        public Mandelzoom(int w, int h) : base(w, h)
        {
            zoomingIn = true;
            // call twice to select two endpoints
            SelectDestination();
            SelectDestination();
        }


        class MandelParameters
        {
            public double CenterX, CenterY;
            public double Zoom;
            public double MaxIterations;
        }

        /// <summary>
        /// Class to handle constant speed pan and zoom
        /// </summary>
        class PanAndZoom
        {

            public MandelParameters src = new MandelParameters(), dst;
            public MandelParameters CurrentParameters = new MandelParameters();

            public void SetNext(double cx, double cy, double zoom, double maxIterations)
            {
                src = dst;
                dst = new MandelParameters {CenterX = cx, CenterY = cy, Zoom = zoom, MaxIterations = maxIterations};
            }

            /// <summary>
            /// As current time ratio goes 0 to 1, compute source to dest parameters
            /// </summary>
            public void Update(double ratio)
            {

                // clamp
                if (ratio < 0) ratio = 0;
                if (ratio > 1.0) ratio = 1.0;
                // want zoom to be constant over time, so needs to be of form
                var del = (dst.Zoom/src.Zoom);
                var df = Math.Pow(del, ratio);
                CurrentParameters.Zoom = src.Zoom*df;

                // iterations follow same pattern
                CurrentParameters.MaxIterations = src.MaxIterations*Math.Pow(dst.MaxIterations/src.MaxIterations, ratio);

                // delta x and delta y for centers follows:
                var factor = del*(1-df)/((1 - del)*df);

                CurrentParameters.CenterX = src.CenterX + factor*(dst.CenterX - src.CenterX);
                CurrentParameters.CenterY = src.CenterY + factor*(dst.CenterY - src.CenterY);
            }
        }

        PanAndZoom panAndZoom = new PanAndZoom();

        private int curInterpolationFrame = 0;
        private int maxInterpolationFrame = 300;
        private bool zoomingIn = false;

        void SelectDestination()
        {
            if (zoomingIn == false)
            {
                var max = MandelbrotSites.Length/4;
                currentMandelIndex = Rand.Next(0, max);
                var i = currentMandelIndex*4;

                panAndZoom.SetNext(MandelbrotSites[i],MandelbrotSites[i+1],1.0/MandelbrotSites[i+2],MandelbrotSites[i+3]);
            }
            else
                panAndZoom.SetNext(0, 0, 1.0, 100);

            curInterpolationFrame = 0;
            zoomingIn = ! zoomingIn;

            // zoom at constant frame rate
            if (panAndZoom.src != null && panAndZoom.dst != null && panAndZoom.dst.Zoom > 0)
            {
                var r = Math.Log(panAndZoom.src.Zoom/panAndZoom.dst.Zoom);
                if (r < 0)
                    r = -r;
                maxInterpolationFrame = (int) (50*r);
            }

        }


        /// <summary>
        /// Index of the mandelbrot site we're zooming into
        /// </summary>
        private int currentMandelIndex = 0;

        private double rotation = 0; // angle in radians of rotation

        /// <summary>
        /// Simple mandelbrot rendering
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angle"></param>
        /// <param name="renderAction"></param>
        void RenderMandelbrot(
            int width, int height, 
            MandelParameters parameters,
            double angle,
            Action<int,int,int> renderAction
            )
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var m = Math.Max(width, height);
            var scale = 2.0/(parameters.Zoom*m);

            double temp;
            for (var i = 0; i < width; ++i)
                for (var j = 0; j < height; ++j)
                {
                    // center pixel
                    var cx = i - width / 2.0;
                    var cy = j - height / 2.0;
                    // zoom in
                    cx *= scale;
                    cy *= scale;
                    // rotate
                    temp = cx*cos + cy*sin;
                    cy   = -cx*sin + cy*cos;
                    cx = temp;
                    // shift
                    cx += parameters.CenterX;
                    cy += parameters.CenterY;

                    // compute iters
                    // z <- z^2+c, which is
                    // x+iy <- (x+iy)^2+(cx+icy), which is
                    // x <- x^2 - y^2 + cx and y <- 2xy+curCenterY
                    double x = cx, y = cy;

                    var count = 0;
                    var maxIters = (int) parameters.MaxIterations;
                    while (count < maxIters)
                    {
                        if (x*x + y*y > 4.0) 
                            break;
                        temp = x*x - y*y + cx;
                        y = 2*x*y + cy;
                        x = temp;
                        ++count;
                    }
                    renderAction(i, j, count);
                }
        }

        private long[] histogram;
        private int[,] counts;

        
        public override void Update()
        {
            base.Update();

            var angle = Frame*2*Math.PI/500.0;
            rotation  = angle;
            //curZoom = (Math.Sin(angle)+1)/2.0;

            curInterpolationFrame++;
            if (curInterpolationFrame >= maxInterpolationFrame)
            {
                curInterpolationFrame = 0;
                SelectDestination();
            }

            var ratio = (double)curInterpolationFrame / maxInterpolationFrame;
            // smooth ends, mapping 0-1 to 0-1
            ratio = (Math.Cos(ratio * Math.PI + Math.PI) + 1) / 2.0;
            panAndZoom.Update(ratio);


            var maxIters = (int) panAndZoom.CurrentParameters.MaxIterations;

            // allocate spaces
            if (histogram == null || histogram.Length <= maxIters)
                histogram = new long[maxIters+1];
            Array.Clear(histogram,0,histogram.Length);
            if (counts == null || counts.GetLength(0) < Width || counts.GetLength(1) < Height)
                counts = new int[Width,Height];

            RenderMandelbrot(
                Width, Height, panAndZoom.CurrentParameters,rotation, 
                (i, j, count) =>
                {
                    histogram[count]++;
                    counts[i, j] = count;
                }
                );

            // histogram coloring
            for (var j = 0; j < Height; ++j)
                for (var i = 0; i < Width; ++i)
                {
                    var count = counts[i, j];
                    if (count == maxIters)
                        SetPixel(i, j, 0, 0, 0);
                    else
                    {
                        double total = histogram.Take(count).Sum();
                        var hue = total/(Width*Height); // scales into 0-1
                        int r, g, b;
                        HslToRgb(hue+(Frame%1000)/1000.0, 1.0, 0.5, out r, out g, out b);
                        SetPixel(i, j, r, g, b);
                    }
                
            }


        }



        /// <summary>
        /// Interesting things to zoom into
        /// Each is center x, center y, width to show, and iteration max
        /// </summary>
static double []  MandelbrotSites =
    {
    // first block are Lomont originals
    0.290006674822,0.6081446209231,7.10329e-005,200,
    -0.831219,-0.234478,0.000377375,170,
    -0.69229,-0.478521,0.000176672,170,
    -0.374247611213,-0.659970174087,1.70354e-008,700,
    -0.56201,-0.642496,0.0126676,500,
    -0.556577,-0.645149,0.00253244,500,
    -0.556853116196,-0.645359365987,7.45798e-007,1000,
    -1.941512,-0.00663435,1.2095e-005,1000,
    -1.941515719003,-0.006632941386,.000000001412,1000,
    -0.694257,0.308663,0.000552741,1000,
    -0.759082,0.0765647,0.00100385,500,
    -0.0888144,-0.654039,1.15516e-005,1000,	




    -0.5,0,2,500,
    -1.25368348, 0.04663671,0.0000005,1024,
    0.296, 0.482165, 0.0055, 1024,
    0.25048714, 0.00001719, 0.00000172, 5120,
    -0.74972518, 0.02760713, 0.00000076, 2048,
    -1.74836938, 0.00000024, 0.00002154, 1024,
    -0.745296 , 0.1130585,0.000484, 1024,
    -0.7454210 , 0.1130485,0.000005, 2048,
    -0.74691 , 0.10725,0.00108, 1024,
    -1.25002 , 0.031172,0.00001, 2048,
    -0.79725736 , 0.17854272,0.00000036, 2048,
    -1.25551983992532956158,0.38255507976198971365,0.00002041824959390845,1024,
    -0.639602,-0.455735,0.0001,2000,
    -1.25186314760508300,0.01945241199478459,0.00025,16384,
    -0.75017936855172060,-0.01161074644406282,6e-6,20000,
    -1.48214325748857200,0.00019673022722551,1e-6,100000,
    -0.09703580219119727,0.65263705725587050,1.9e-7,20000,
    -0.09703580219119727,0.65263705725587060,1.91e-7,10000,
    -1.94156372319501500,0.00015082209072200,1.34e-7,3000,
    -0.75017938225569000,-0.01161072996831062,8.2e-7,100000,
    -0.09703580179264795,0.65263705782502150,3.289e-8,40000,
    -1.94156372027722900,0.00015081593477735,1.608e-8,3000,
    -1.62411052742159200,0.00011081995839211,4.89e-9,2047,
    -1.48214335931301600,0.00019668350231959,8.7e-9,100000,
    -1.94156370514768100,0.00015081169268077,4.7e-9,3000,
    -1.94156372034964600,0.00015081454611931,2.658e-9,3000,
    -1.94153831861436800,-0.00002254957645940,2.31e-9,16384,
    -1.76862112336503300,0.00183956127603124,2.77e-10,32750,
    -1.94156372039606100,0.00015081529536306,6.92e-11,3000,
    -0.20946935454112780,1.11185604439279200,2.9292e-12,65536,
// slow ones, removed
//	-0.75017954826745840,-0.01161053694982410,5.585e-11,100000,
//	-0.75017954826734170,-0.01161053695008511,5.66e-12,100000,
//	-0.11170741810190090,0.65023396454887390,1.13e-10,8888,
//	-0.75017954823748270,-0.01161053697134931,2.42e-9,100000,
//	-0.09703580179264795,0.65263705782502150,3.288e-8,30000,
    0,0,0,0 // end of list
    };

    }
}