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
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hypnocube.Demo.Model.Demos
{
    public class Surface
    {
        public Surface(int w, int h)
        {
            Width = w;
            Height = h;
            Image = new uint[w*h];
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        ///     image in BGRA, width then height
        /// </summary>
        public uint[] Image { get; private set; }

        public static Surface FromMemoryPNG(byte[] data)
        {
            Surface surface = null;
            using (var stream = new MemoryStream(data))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                var w = bitmap.PixelWidth;
                var h = bitmap.PixelHeight;
                Debug.Assert(bitmap.Format == PixelFormats.Bgra32);

                surface = new Surface(w, h);
                var p = new byte[w*h*4];
                bitmap.CopyPixels(p, w*4, 0);
                Buffer.BlockCopy(p, 0, surface.Image, 0, p.Length);
            }
            return surface;
        }


        public void GetPixel(int i, int j, out int red, out int green, out int blue)
        {
            red = green = blue = 0;
            if (i < 0 || j < 0 || Width <= i || Height <= j)
                return;
            var bgra = Image[i + j*Width];
            red = (int) (bgra >> 16) & 255;
            green = (int) (bgra >> 8) & 255;
            blue = (int) (bgra >> 0) & 255;
        }

        public bool SetPixel(int i, int j, int red, int green, int blue)
        {
            if (i < 0 || j < 0 || Width <= i || Height <= j)
                return false;

            var bgra = (uint) (blue << 0) + (uint) (green << 8) + (uint) (red << 16) + 0xff000000;
            Image[i + j*Width] = bgra;
            return true;
        }

        public void MaxPixel(int i, int j, int red, int green, int blue)
        {
            if (i < 0 || j < 0 || Width <= i || Height <= j)
                return;
            int r, g, b;
            GetPixel(i, j, out r, out g, out b);
            SetPixel(i, j, Math.Max(r, red), Math.Max(g, green), Math.Max(b, blue));
        }

        /// <summary>
        ///     Fade to the selected color, to the given amount (0=none, 1 = fully)
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="amount"></param>
        public void Fade(int red, int green, int blue, double amount)
        {
            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                {
                    int r, g, b;
                    GetPixel(i, j, out r, out g, out b);
                    r = (int) (red*amount + r*(1 - amount));
                    g = (int) (green*amount + g*(1 - amount));
                    b = (int) (blue*amount + b*(1 - amount));
                    SetPixel(i, j, r, g, b);
                }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, int red, int green, int blue)
        {
            DrawLine(x0, y0, x1, y1, (i, j) => SetPixel(i, j, red, green, blue));
        }

        public void DrawLine(int x0, int y0, int x1, int y1, Action<int, int> drawPixel)
        {
            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            int sx, sy;
            if (x0 < x1) sx = 1;
            else sx = -1;
            if (y0 < y1) sy = 1;
            else sy = -1;
            var err = dx - dy;

            while (true)
            {
                drawPixel(x0, y0);
                if (x0 == x1 && y0 == y1) break;
                var e2 = 2*err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                if (x0 == x1 && y0 == y1)
                {
                    drawPixel(x0, y0);
                    break;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }


        /// <summary>
        ///     Each value in 0-1
        /// </summary>
        /// <param name="h">Value in 0-1</param>
        /// <param name="s">Value in 0-1</param>
        /// <param name="l">Value in 0-1</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
        {
            double rd, gd, bd;
            HslToRgb(h, s, l, out rd, out gd, out bd);
            r = (int) (rd*255);
            g = (int) (gd*255);
            b = (int) (bd*255);
        }


        /// <summary>
        ///     Given a hue in 0-1, scales internal values such that
        ///     colors are visually more evenly represented.
        ///     Power 1 is default.
        /// </summary>
        /// <param name="hue"></param>
        /// <returns></returns>
        public static double ScaleHue(double hue, double power = 0.5)
        {
            // the idea is to keep the 6 base colors at 0,1/6,2/6,etc fixed, and make 
            // the odd value more represented, since they are not.
            // For example, in a hue cycle, yellow is much shorter than red

            // make sure h in [0,1)
            if (hue < 0)
                hue += -Math.Floor(hue);
            hue = hue - Math.Floor(hue);


            var hInt = (int) Math.Floor(6*hue);
            var hFrac = hue*6 - hInt;
            var hOdd = (hInt & 1) != 0;
            var hNew = hInt + (hOdd ? Math.Pow(hFrac, power) : Math.Pow(hFrac, 1.0/power));
            return hNew/6;
        }

        // each value in 0-1
        public static void HslToRgb(double h, double s, double l, out double r, out double g, out double b)
        {
            // make sure h in [0,1)
            if (h < 0)
                h += -Math.Floor(h);
            h = h - Math.Floor(h);


            if (Math.Abs(s) < 0.00001)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                var c = (1 - Math.Abs(2*l - 1))*s; // chroma
                var hp = 6*h;
                var ab = (hp/2 - Math.Floor(hp/2))*2; // hp mod 2
                var x = c*(1 - Math.Abs(ab - 1));
                r = g = b = 0;
                if (hp < 1)
                {
                    r = c;
                    g = x;
                }
                else if (hp < 2)
                {
                    r = x;
                    g = c;
                }
                else if (hp < 3)
                {
                    g = c;
                    b = x;
                }
                else if (hp < 4)
                {
                    g = x;
                    b = c;
                }
                else if (hp < 5)
                {
                    b = c;
                    r = x;
                }
                else if (hp < 6)
                {
                    b = x;
                    r = c;
                }
                var m = l - c*0.5;
                r += m;
                g += m;
                b += m;
            }
        }

        /// <summary>
        ///     draw a raytraced sphere
        /// </summary>
        public void VectorBall(double x, double y, double radius, int r, int g, int b, double nx = 0, double ny = 0,
            double nz = 1)
        {
            for (var i = -radius; i <= radius; ++i)
                for (var j = -radius; j <= radius; ++j)
                {
                    if (i*i + j*j > radius*radius)
                        continue;
                    var dx = i/radius;
                    var dy = j/radius;
                    var dz = Math.Sqrt(Math.Max(1 - dx*dx - dy*dy, 0));

                    // color based on normal, white (1) to color (1/2) to black (0)
                    var alpha = dx*nx + dy*ny + dz*nz; // dot product
                    if (alpha < 0)
                        alpha = 0; // on dark side, set to lowest color
                    Debug.Assert(0 <= alpha && alpha <= 1.0);
                    double r1, g1, b1;
                    if (alpha > 0.5)
                    {
                        // blend white (1) to color (0)
                        alpha = (alpha - 0.5)*2;
                        alpha = Math.Pow(alpha, 5.0); // make more specular
                        r1 = r*(1 - alpha) + 255*alpha;
                        g1 = g*(1 - alpha) + 255*alpha;
                        b1 = b*(1 - alpha) + 255*alpha;
                    }
                    else
                    {
                        // blend color (1/2) to black (0)
                        alpha = (alpha)*2;
                        //alpha = Math.Pow(alpha, 3.0);// make fade faster
                        r1 = r*alpha;
                        g1 = g*alpha;
                        b1 = b*alpha;
                    }
                    //r1 = g1 = b1 = 255*alpha;
                    SetPixel((int) (i + x), (int) (j + y), (int) (r1), (int) (g1), (int) (b1));
                }
        }

        /// <summary>
        ///     Sort balls by depth and render
        /// </summary>
        /// <param name="balls"></param>
        public void DrawVectorBalls(List<Ball> balls)
        {
            balls.Sort((a, b) => -a.Point.Z.CompareTo(b.Point.Z));
            var cx = Width/2;
            var cy = Height/2;
            foreach (var p in balls)
            {
                VectorBall(p.Point.X + cx, p.Point.Y + cy, p.Radius, p.Color.Red, p.Color.Green, p.Color.Blue);
            }
        }


        public void Fill(int red, int blue, int green)
        {
            for (var i = 0; i < Width; ++i)
                for (var j = 0; j < Height; ++j)
                    SetPixel(i, j, red, green, blue);
        }
    }
}