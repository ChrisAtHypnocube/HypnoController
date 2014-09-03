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
using System.Linq;

namespace Hypnocube.Demo.Model.Demos
{
    internal class Maze : DemoBase
    {
        private readonly Plasma plasma;

        private List<Actor> actors = new List<Actor>();
        private int framesLeft;
        private MazeGenerator gen;

        // true for color cell solid
        private bool[,] grid;

        private int lastInfectedFrame;

        public Maze(int w, int h)
            : base(w, h)
        {
            Generate();
            plasma = new Plasma(w, h);
        }

        public override void Reset()
        {
            base.Reset();
            Generate();
        }

        private void Generate()
        {
            framesLeft = 0;

            gen = new MazeGenerator();

            try
            {
                Action<string> output = s => Debug.WriteLine(s);
                output = s => { }; // do nothing
                var dx = Width/2 + 1;
                var dy = Height/2 + 1;
                gen.SetProgressFunction(output);
                gen.Create(0, dx - 1, dx, 1, dy - 1, dy, 0, 0, 1);
                grid = new bool[Width + 1, Height + 1];

                Action<int, int> set = (a, b) => grid[a, b] = true;

                for (var x = 0; x < Width/2; ++x)
                    for (var y = 0; y < Height/2; ++y)
                    {
                        var val = (int) gen.CellWalls(x + 1, y + 1);
                        output(val.ToString());

                        for (var i = 0; i < 3; ++i)
                            for (var j = 0; j < 3; ++j)
                            {
                                var sx = x*2 + i;
                                var sy = y*2 + j;
                                if (i == 0 && j == 1)
                                {
                                    if ((val & 8) != 0)
                                        set(sx, sy);
                                }
                                else if (i == 2 && j == 1)
                                {
                                    if ((val & 2) != 0)
                                        set(sx, sy);
                                }
                                else if (i == 1 && j == 0)
                                {
                                    if ((val & 1) != 0)
                                        set(sx, sy);
                                }
                                else if (i == 1 && j == 2)
                                {
                                    if ((val & 4) != 0)
                                        set(sx, sy);
                                }
                                else if (i != 1 && j != 1)
                                    set(sx, sy);
                            }
                    }

                actors = new List<Actor>();
                for (var i = 0; i < Width*Height/25; ++i)
                {
                    var z = new Actor();
                    z.dir = Rand.Next(4);
                    do
                    {
                        z.x = Rand.Next(Width);
                        z.y = Rand.Next(Height);
                    } while (grid[z.x, z.y]);
                    actors.Add(z);
                }

                // set some zombies
                for (var i = 0; i < Width*Height/40 + 1; ++i)
                    actors[Rand.Next(actors.Count)].Type = ActorType.Zombie;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public override void Update()
        {
            base.Update();
            if ((Frame%5) == 0)
                plasma.Update();

            if ((Frame & 1) != 0)
                return;

            Fill(0, 0, 0);

            for (var x = 0; x < Width; ++x)
                for (var y = 0; y < Height; ++y)
                {
                    if (grid[x, y])
                    {
                        int r, g, b;
                        plasma.GetPixel(x, y, out r, out g, out b);
                        r = (r + 0)/2;
                        g = (g + 0)/2;
                        b = (b + 0)/2;
                        SetPixel(x, y, r, g, b);
                    }
                }

            var humansLeft = actors.Any(a => a.Type == ActorType.Human);

            foreach (var a in actors)
            {
                var moves = GetMoves(a);

                // compute move direction
                switch (a.Type)
                {
                    case ActorType.Human:
                        moves.RemoveAll(d => Sees(a, d, ActorType.Zombie));
                        if (moves.Count > 1)
                            moves.Remove((a.dir + 2) & 3); // remove opposite if enough moves
                        if (moves.Count == 0)
                            moves.Add(5); // none
                        break;
                    case ActorType.Zombie:
                        var humans = moves.Where(d => Sees(a, d, ActorType.Human)).ToList();
                        if (humans.Count > 0)
                            moves = humans; // chase a human
                        else if (moves.Count > 1)
                            moves.Remove((a.dir + 2) & 3); // remove opposite if enough
                        break;
                    case ActorType.Blood:
                        moves = new List<int> {5}; // none
                        break;
                }
                a.dir = moves[Rand.Next(moves.Count)];
                int dx, dy;
                GetDir(a.dir, out dx, out dy);

                if (humansLeft)
                {
                    // only move if humans left
                    if (a.Type == ActorType.Human || (Frame & 2) == 0)
                    {
                        a.x = (a.x + dx + Width)%Width;
                        a.y = (a.y + dy + Height)%Height;
                    }
                }


                // check if zombied
                foreach (var z in actors)
                    if (z.x == a.x && z.y == a.y && a.Type != z.Type)
                    {
                        // collision, check them
                        if (a.Type == ActorType.Human && z.Type == ActorType.Zombie)
                        {
                            a.Type = ActorType.Blood;
                            a.frame = lastInfectedFrame = Frame;
                        }
                        if (a.Type == ActorType.Zombie && z.Type == ActorType.Human)
                        {
                            z.Type = ActorType.Blood;
                            z.frame = lastInfectedFrame = Frame;
                        }
                    }

                // draw
                if (a.Type == ActorType.Human)
                    SetPixel(a.x, a.y, 2*239/3, 2*208/3, 2*207/3); // human color
                else if (a.Type == ActorType.Zombie)
                    SetPixel(a.x, a.y, 0, 255, 0); // zombie color
                else if (a.Type == ActorType.Blood)
                {
                    var df = Frame - a.frame;
                    if (df > 255/4)
                    {
                        a.frame = 0;
                        a.Type = ActorType.Zombie;
                    }
                    SetPixel(a.x, a.y, 255 - df*4, 0, 0);
                }
            }

            // reset if all humans are gone for too many frames
            if (framesLeft == 0 && !actors.Any(a => a.Type == ActorType.Human))
            {
                framesLeft = 100;
            }

            if (framesLeft > 0)
            {
                framesLeft--;
                if (framesLeft == 0)
                    Generate();
            }

            if (Frame - lastInfectedFrame > 1000)
            {
                lastInfectedFrame = Frame + 50;
                Generate();
            }
        }

        private void GetDir(int dir, out int dx, out int dy)
        {
            dx = dy = 0;
            if (dir == 0) dy = -1;
            else if (dir == 1) dx = 1;
            else if (dir == 2) dy = 1;
            else if (dir == 3) dx = -1;
        }

        private bool Sees(Actor a, int dir, ActorType actorType)
        {
            int dx, dy;
            GetDir(dir, out dx, out dy);

            var looker = actors.Where(a1 => a1.Type == actorType).ToList();

            var x = a.x;
            var y = a.y;

            do
            {
                x = (x + dx + Width)%Width;
                y = (y + dy + Height)%Height;
                if (looker.Any(a1 => a1.x == x & a1.y == y))
                    return true;
            } while (!grid[x, y] && (x != a.x || y != a.y));
            return false;
        }

        private List<int> GetMoves(Actor a)
        {
            var ans = new List<int>();
            int dx, dy;
            for (var d = 0; d < 4; ++d)
            {
                GetDir(d, out dx, out dy);
                var x = (a.x + dx + Width)%Width;
                var y = (a.y + dy + Height)%Height;
                if (!grid[x, y])
                    ans.Add(d);
            }
            return ans;
        }

        #region Nested type: Actor

        private class Actor
        {
            // location and direction

            // 0  human, 1 = zombie, 2 = Blood
            public ActorType Type;
            public int dir; // 0,1,2,3 are up,right,down,left, 5 is none 

            // count age
            public int frame;
            public int x, y;
        }

        #endregion

        #region Nested type: ActorType

        private enum ActorType
        {
            Human,
            Zombie,
            Blood
        }

        #endregion
    }
}