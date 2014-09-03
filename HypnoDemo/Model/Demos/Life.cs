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
    internal class Life : DemoBase
    {
        // board is a grid of cells which contain alive state and age
        private readonly Cell[,] board1;
        private readonly Cell[,] board2;
        private int genesisThreshold = 100; // age before new life starts in dead cells
        private Cell[,] lastFilled;
        private double lifeThreshold = 0.40;
        // number of tribes
        private int tribeCount = 3;

        public Life(int w, int h)
            : base(w, h)
        {
            board1 = new Cell[w, h];
            board2 = new Cell[w, h];
            // create board cells
            for (var j = 0; j < Height; ++j)
                for (var i = 0; i < Width; ++i)
                {
                    board1[i, j] = new Cell();
                    board2[i, j] = new Cell();
                }

            // attach neighbors
            // deltas, up then clockwise
            var dx = new[] {0, 1, 1, 1, 0, -1, -1, -1};
            var dy = new[] {-1, -1, 0, 1, 1, 1, 0, -1};
            for (var j = 0; j < Height; ++j)
                for (var i = 0; i < Width; ++i)
                {
                    for (var k = 0; k < 8; ++k)
                    {
                        var i2 = (i + dx[k] + Width)%Width; // wraparound
                        var j2 = (j + dy[k] + Height)%Height; // wraparound
                        board1[i, j].Neighbors[k] = board1[i2, j2];
                        board2[i, j].Neighbors[k] = board2[i2, j2];
                    }
                }
            lastFilled = board1;
            RandomBoard(lastFilled);
        }

        private void RandomBoard(Cell[,] board)
        {
            for (var j = 0; j < Height; ++j)
                for (var i = 0; i < Width; ++i)
                {
                    board[i, j].Alive = Rand.NextDouble() > lifeThreshold;
                    board[i, j].Age = 0; // start here
                    board[i, j].Tribe = Rand.Next(tribeCount);
                }
        }


        public override void Update()
        {
            var w = Width;
            var h = Height;

            if ((Frame & 127) != 0)
                return;

            // update state
            SingleStep();

            // draw board
            for (var i = 0; i < w; ++i)
                for (var j = 0; j < h; ++j)
                {
                    int r, g, b;
                    GetCellColor(lastFilled[i, j], out r, out g, out b);
                    SetPixel(i, j, r, g, b);
                }
        }

        /// <summary>
        ///     How a cell gets colored
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        private void GetCellColor(Cell cell, out int r, out int g, out int b)
        {
            r = g = b = 0; // assume dead
            if (cell.Alive && cell.Tribe == 0)
                r = 255;
            if (cell.Alive && cell.Tribe == 1)
                g = 255;
            if (cell.Alive && cell.Tribe == 2)
                b = 255;
        }

        private void SingleStep()
        {
            var src = lastFilled == board1 ? board1 : board2;
            var dst = lastFilled == board1 ? board2 : board1;
            lastFilled = dst;
            var tribeCounts = new int[tribeCount];
            for (var j = 0; j < Height; ++j)
                for (var i = 0; i < Width; ++i)
                {
                    var cell = src[i, j];
                    var alive = cell.Alive;
                    var nbrs = cell.Neighbors.Sum(nbr => nbr.Alive ? 1 : 0);

                    // get tribe counts
                    Array.Clear(tribeCounts, 0, tribeCount);
                    for (var k = 0; k < cell.Neighbors.Length; ++k)
                        tribeCounts[cell.Neighbors[k].Tribe]++;


                    // default state is to leave alone, just age it once
                    dst[i, j].Set(cell.Alive, cell.Age + 1, cell.Tribe);

                    if (alive && nbrs < 2)
                    {
                        // cell dies from too few neighbors
                        dst[i, j].Alive = false;
                        dst[i, j].Age = 0;
                    }
                    if (alive && 2 <= nbrs && nbrs <= 3)
                    {
                        // cell lives on - todo - let tribes mix over time?
                    }
                    if (alive && 3 < nbrs)
                    {
                        // cell dies from overcrowding
                        dst[i, j].Alive = false;
                        dst[i, j].Age = 0;
                    }
                    if (!alive && nbrs == 3)
                    {
                        // enough neighbors, reproduces
                        dst[i, j].Alive = true;
                        dst[i, j].Age = 0;
                        // if all neighbors same type, then same tribe, else random tribe
                        var max = tribeCounts.Max();
                        if (max == 3)
                            dst[i, j].Tribe = Array.IndexOf(tribeCounts, max);
                        else
                            dst[i, j].Tribe = Rand.Next(tribeCount);
                    }

                    // finally, genesis option adds new life
                    if (!dst[i, j].Alive && dst[i, j].Age > genesisThreshold && Rand.NextDouble() > lifeThreshold)
                    {
                        dst[i, j].Alive = true;
                        dst[i, j].Age = 0;
                        dst[i, j].Tribe = Rand.Next(tribeCount);
                    }
                }
        }

        #region Nested type: Cell

        private class Cell
        {
            public Cell()
            {
                Neighbors = new Cell[8];
            }

            public bool Alive { get; set; }

            /// <summary>
            ///     Age is how long alive or how long dead
            /// </summary>
            public int Age { get; set; }

            /// <summary>
            ///     An integer tribe, used for coloring
            /// </summary>
            public int Tribe { get; set; }

            /// <summary>
            ///     Neighbors, start up, then clockwise
            /// </summary>
            public Cell[] Neighbors { get; private set; }

            internal void Set(bool alive, int age, int tribe)
            {
                Alive = alive;
                Age = age;
                Tribe = tribe;
            }
        }

        #endregion
    }
}