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

namespace Hypnocube.Demo.Model.Demos
{
    internal class Scrolly2 : DemoBase
    {
        private readonly FontController font;

        private readonly Plasma plasma;
        private int curPos;

        private int resetCount;
        private int scrollStart = Int32.MinValue;
        private string text = "";

        public Scrolly2(int w, int h)
            : base(w, h)
        {
            plasma = new Plasma(w, h);
            plasma.Update();

            font = new FontController();
            curPos = Width;

            ParameterText = ",   Hypnocube LED Strand Driver demo    ,   Hypnocube - www.Hypnocube.com       , All Hail Hypnocube! ";
            ParameterDescription = "First char is separator, then strings to show, separated by the separator";
        }

        public override void Reset()
        {
            base.Reset();
            ++resetCount;

            var texts = Parameters();
            if (texts.Any())
                text = texts[resetCount % texts.Count];
            scrollStart = Int32.MinValue;
        }


        public override void Update()
        {
            base.Update();
            if (resetCount == 0)
                Reset(); // need at least one reset

            if (true)
            {
                //if ((frame & 3) == 0)
                plasma.Update();

                for (var j = 0; j < Height; ++j)
                    for (var i = 0; i < Width; ++i)
                    {
                        int r, g, b;
                        plasma.GetPixel(i, j, out r, out g, out b);
                        SetPixel(i, j, r, g, b);
                    }
            }
            else
            {
                Fill(0, 0, 0);
            }

            var xpos = curPos;
            var ypos = Height/2 - font.Font.LineHeight/2;
            var gap = (Height - font.Font.LineHeight)*0.6;

            curPos -= 2;

            font.Render(ref xpos, ref ypos,
                text,
                Width, Height,
                (i, j, r, g, b, a) =>
                {
                    if (a != 0)
                    {
                        int r1, g1, b1;
                        var j2 = (int) (j + Math.Sin(Frame/8.0 + i*4.0/Width)*gap);
                        GetPixel(i, j2, out r1, out g1, out b1);

                        //j2 = j;
                        r1 = a*r/255 + (255 - a)*r1/255;
                        g1 = a*g/255 + (255 - a)*g1/255;
                        b1 = a*b/255 + (255 - a)*b1/255;
                        SetPixel(i, j2, r1, g1, b1);
                    }
                }
                );

            if (xpos < 0)
                curPos = Width;


#if false
            if (scrollStart == int.MinValue)
                scrollStart = Width - 20;
            int i = scrollStart, j = 0; // where to draw
            scrollStart--;

            //            while (i < 0)
            //            {
            //                i += Width;
            //            }

            Fill(0, 0, 0);
            var seen = false;
            var mono = false;
            var alpha = true;

            var startLocations = Font1.ColorFontStartLocations;
            var fontData = Font1.ColorFontData;
            alpha = false;

            //startLocations = Font1.Consolas16StartLocations;
            //fontData = Font1.Consolas16Data;

            //startLocations = Font1.GunnStartLocations;
            //fontData = Font1.GunnData;
            //alpha = true;

            foreach (var c in text)
            {
                var index = startLocations[c - 32];
                var dx = fontData[index++]; // size
                var dy = fontData[index++];
                var sx = fontData[index++]; // start offset
                var sy = fontData[index++];

                for (var y = 0; y < dy; ++y)
                    for (var x = 0; x < dx; ++x)
                    {
                        int colorR, colorG, colorB, colorA = 0;
                        if (mono)
                            colorR = colorG = colorB = fontData[index++];
                        else
                        {
                            colorR = fontData[index++];
                            colorG = fontData[index++];
                            colorB = fontData[index++];
                            if (alpha)
                                colorA = fontData[index++];
                            else
                                colorA = 255;

                        }
                        if (colorR != 0 || colorG != 0 || colorB != 0 || colorA != 0)
                        {
                            int r, g, b;
                            var xx = x + i + sx;
                            var yy = y + j + sy;
                            plasma.GetPixel(xx, yy, out r, out g, out b);
                            //r = (r + colorR) / 2;
                            //g = (g + colorG) / 2;
                            //b = (b + colorB) / 2;
                            //r = colorR;
                            //g = colorG;
                            //b = colorB;
                            r = r * colorR / 256;
                            g = g * colorG / 256;
                            b = b * colorB / 256;
                            seen |= SetPixel(xx, yy, r, g, b);
                        }
                    }
                if (c != ' ')
                    i += dx + 1;
                else
                {
                    i += 16; // todo - font width
                }
            }

            if (!seen)
                scrollStart = Width - 20;
#endif
        }
    }
}