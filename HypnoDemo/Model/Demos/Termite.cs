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
using System.Linq;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Termite : DemoBase
    {
        private readonly List<TermiteStruct> termites = new List<TermiteStruct>();

        private byte[,] termiteBoard;

        public Termite(int w, int h) : base(w, h)
        {
        }

        public override void Update()
        {
            var w = Width;
            var h = Height;
            if (termiteBoard == null || termiteBoard.GetLength(0) != w || termiteBoard.GetLength(1) != h)
            {
                // board is 0 where filled, 1 where there is a termite, 2 where a tunnel has been dug
                termiteBoard = new byte[w, h];
                for (var i = 0; i < w*h/100; ++i)
                {
                    var t = new TermiteStruct();
                    t.x = Rand.Next(w);
                    t.y = Rand.Next(h);
                    t.r = Rand.Next(256);
                    t.g = Rand.Next(256);
                    t.b = Rand.Next(256);
                    termiteBoard[t.x, t.y] = 1;
                    termites.Add(t);
                }
            }

            // update termites
            foreach (var t in termites)
            {
                if (t.dx == 0 && t.dy == 0)
                {
                    // start off termite
                    if (Rand.Next(100) > 50)
                        t.dx = Rand.Next(2)*2 - 1;
                    else
                        t.dy = Rand.Next(2)*2 - 1;
                    t.length = Rand.Next(10) + 5;
                }
                termiteBoard[t.x, t.y] = 2; // erase it
                t.x = (t.x + t.dx + w)%w;
                t.y = (t.y + t.dy + h)%h;
                t.length--;
                if (t.length <= 0)
                    t.dx = t.dy = 0;
                termiteBoard[t.x, t.y] = 1;
            }

            // draw board
            for (var i = 0; i < w; ++i)
                for (var j = 0; j < h; ++j)
                {
                    int r, g, b;
                    if (termiteBoard[i, j] == 0)
                        r = g = b = 255;
                    else if (termiteBoard[i, j] == 2)
                        r = g = b = 0;
                    else
                    {
                        var t = termites.FirstOrDefault(t1 => t1.x == i && t1.y == j);
                        r = t.r;
                        g = t.g;
                        b = t.b;
                    }
                    SetPixel(i, j, r, g, b);
                }
        }

        #region Nested type: TermiteStruct

        /// <summary>
        ///     A termite digs in given direction until end reached, then randomly turns
        ///     If a new tunnel is about to be connected, then usually randomly turn before
        /// </summary>
        public class TermiteStruct
        {
            // position

            // desired direction
            public int b;
            public int dx, dy;
            public int g;

            // desired steps left
            public int length;

            // color
            public int r;
            public int x, y;
        }

        #endregion
    }
}