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
    public class MazeGenerator
    {
        #region Public

        public MazeGenerator()
        {
            SizeX = 0;
            SizeY = 0;
            SizeZ = 0;
            max_run = 10;
            ProgressFunc1 = null;
        }

        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public int SizeZ { get; private set; }

        public class Cell
        {
            public cell_attr Attribute;

            public override string ToString()
            {
                return Attribute.ToString();
            }
        }

        public Cell[,,] Cells { get; private set; }

        /// <summary>
        ///     create 2D or 3D maze, given start, finish coords in range [0 - (sx-1)], etc
        /// </summary>
        /// <param name="startX1"></param>
        /// <param name="finishX1"></param>
        /// <param name="sizeX1"></param>
        /// <param name="startY1"></param>
        /// <param name="finishY1"></param>
        /// <param name="sizeY1"></param>
        /// <param name="startZ1"></param>
        /// <param name="finishZ1"></param>
        /// <param name="sizeZ1"></param>
        public void Create(
            int startX1,
            int finishX1,
            int sizeX1,
            int startY1,
            int finishY1,
            int sizeY1,
            int startZ1,
            int finishZ1,
            int sizeZ1
            )
        {
            // todo - check inputs
            start_x = startX1;
            start_y = startY1;
            start_z = startZ1;
            finish_x = finishX1;
            finish_y = finishY1;
            finish_z = finishZ1;
            SizeX = sizeX1;
            SizeY = sizeY1;
            SizeZ = sizeZ1;
            InitCells();

            var text_out = 0; // counter

            // algorithm:
            //   1. connect start to finish directly by random lengths - todo - add some backwards paths - perhaps step 1.5?
            //   2. walk path, randomly add perturbations
            //   3. walk path, add long dead ends from path
            //   4. while not all cells connected, go through and remove one wall if neighboring connected cell

            int i, j, k; // indices counters
            int dx, dy, dz;
            var current = new point3D(start_x, start_y, start_z); // current point
            var final = new point3D(finish_x, finish_y, finish_z); // finish point
            var start = new point3D(start_x, start_y, start_z);

            // step 1
            while (current != final)
            {
                // used to weight probability of x/y/z movement
                var val = rand.Next(SizeX + SizeY + SizeZ);
                // go random distance
                if ((current.x != final.x) && (val < SizeX))
                {
                    // dist to go
                    dx = Math.Min(rand.Next(Math.Abs(start.x - final.x)), Math.Abs(current.x - final.x));
                    if (dx > 10)
                        dx /= 4;
                    dx %= max_run; // make sure not too long
                    if (dx == 0)
                        dx = 1;
                    while (dx-- > 0)
                    {
                        if (current.x > finish_x)
                        {
                            RemoveWalls(cell_attr.West, current);
                            current.x--;
                        }
                        else if (current.x < finish_x)
                        {
                            RemoveWalls(cell_attr.East, current);
                            current.x++;
                        }
                    }
                }
                else if ((current.y != final.y) && (val < SizeX + SizeY) && (val >= start_x))
                {
                    // dist to go
                    dy = Math.Min(rand.Next(Math.Abs(start.y - final.y)), Math.Abs(current.y - final.y));
                    if (dy > 10)
                        dy /= 4;
                    dy %= max_run; // make sure not too long
                    if (dy == 0)
                        dy = 1;
                    while (dy-- > 0)
                    {
                        if (current.y > final.y)
                        {
                            RemoveWalls(cell_attr.North, current);
                            current.y--;
                        }
                        else if (current.y < final.y)
                        {
                            RemoveWalls(cell_attr.South, current);
                            current.y++;
                        }
                    }
                }
                else if ((current.z != final.z) && (val >= start_x + start_y))
                {
                    // dist to go
                    dz = Math.Min(rand.Next(Math.Abs(start.z - final.z)), Math.Abs(current.z - final.z));
                    if (dz > 10)
                        dz /= 4;
                    dz %= max_run; // make sure not too long
                    if (dz == 0)
                        dz = 1;
                    while (dz-- > 0)
                    {
                        if (current.z > final.z)
                        {
                            RemoveWalls(cell_attr.Down, current);
                            current.z--;
                        }
                        else if (current.z < final.z)
                        {
                            RemoveWalls(cell_attr.Up, current);
                            current.z++;
                        }
                    }
                }
            }

            if (((text_out++) & 1023) == 0)
            {
                ProgressFunc("Step 1/4, ({0},{1},{2})->({3},{4},{5})           ", current.x, current.y, current.z,
                    finish_x, finish_y,
                    finish_z);
            }

            // list of coords that are connected to maze
            var found_coords = new List<point3D>(); // this will store (i,j,k) triples of set coords

            // mark found so far
            for (i = 0; i < SizeX; i++)
                for (j = 0; j < SizeY; j++)
                    for (k = 0; k < SizeZ; k++)
                        if (ReadCell(i, j, k) != cell_attr.All)
                            found_coords.Add(new point3D(i, j, k));

            // step 2 - walk maze, add perturbations
            int numAdd;
            for (var pert = 0; pert < 10; pert++)
            {
                const int maxPerturbations = 6; // how many items
                int[] vals = {400, 200, 50, 50, 50, 50}; // divisors of how many to do
                // multipliers
                double[] min_vals = {0.3, 0.1, 0, 0, 0, 0};
                double[] max_vals = {0.4, 0.2, 0.1, 0.1, 0.1, 0.1};

                numAdd = SizeX*SizeY*SizeZ/vals[pert%maxPerturbations]; // todo - const an input to the method?

                var maxDim = Math.Max(Math.Max(SizeX, SizeY), SizeZ);

                while (numAdd-- > 0)
                {
                    // add large perturbations
                    var added = false;
                    var loopCount = 0;

                    while ((added == false) && (loopCount < 1000))
                    {
                        loopCount++; // prevent infinite loops
                        added = true; // assume adds this pass

                        // find a start cell
                        Debug.Assert(found_coords.Count != 0);
                        var pos = rand.Next(found_coords.Count);
                        current = new point3D(found_coords[pos]);
                        Debug.Assert(ReadCell(current.x, current.y, current.z) != cell_attr.All);

                        // see if we added a perturbation
                        added = AddPertubation(current,
                            (int)
                                (min_vals[pert%maxPerturbations]*maxDim),
                            (int)
                                (max_vals[pert%maxPerturbations]*maxDim),
                            found_coords);
                    } // while one not added

                    if (((text_out++) & 1023) == 0)
                    {
                        ProgressFunc("Step 2/4 perturbations, ({0},{1})->({2},{3})           ", pert, numAdd, 6, 0);
                    }
                } // do until enough perturbations added
            } // for enough perturbation passes

            // fill in solution
            // set bit as seen
            for (var pos = 0; pos < found_coords.Count; pos ++)
                data[found_coords[pos].x, found_coords[pos].y, found_coords[pos].z] |= 1;

            // step 3 - all long dead ends
            numAdd = found_coords.Count;
            while (numAdd-- > 0)
            {
                bool added;
                added = false;
                var pass = 0;
                while ((added == false) && (pass++ < 100))
                {
                    added = true; // assume adds this pass

                    // find a start cell
                    Debug.Assert(found_coords.Count != 0);
                    var pos = rand.Next(found_coords.Count);
                    current = new point3D(found_coords[pos]);
                    Debug.Assert(ReadCell(current.x, current.y, current.z) != cell_attr.All);

                    // see if we added a perturbation
                    added = AddDeadEnd(current.x, current.y, current.z, found_coords);
                } // while one not added
                if (added == false) // could not find any! - jump out now
                    numAdd = 0;

                if (((text_out++) & 1023) == 0)
                {
                    ProgressFunc("Step 3/4 dead ends, {0}           ", numAdd);
                }
            } // do until enough perturbations added

            // show dead ends
            for (var pos = 0; pos < found_coords.Count; pos ++)
                data[found_coords[pos].x, found_coords[pos].y, found_coords[pos].z] |= 2;


            // step 4 - connect neighboring cells till all connected
            // put unadded in list, remove nodes as added, and walk list till all found.... - faster for large mazes
            var unmarked = new Queue<point3D>(); // (i,j,k) coords of unmarked nodes, compressed
            Debug.Assert((SizeX*SizeZ) < (1L << 31)/SizeY); // Debug.Assert in range for compacting
            try
            {
                for (k = 0; k < SizeZ; k++)
                    for (i = 0; i < SizeX; i++)
                        for (j = 0; j < SizeY; j++)
                            if (ReadCell(i, j, k) == cell_attr.All)
                                unmarked.Enqueue(new point3D(i, j, k));
            }
            catch (Exception )
            {
                ProgressFunc("Error creating untouched list - probably out of memory");
            }

            text_out = 0;

            while (unmarked.Any())
            {
                if (((text_out++) & 1023) == 0)
                {
                    ProgressFunc("Step 4/4 connect all, {0} {1}           ", unmarked.Count, text_out);
                }

                while (unmarked.Any())
                {
                    var temp = unmarked.Dequeue();
                    // unpack it
                    i = temp.x;
                    j = temp.y;
                    k = temp.z;
                    var added = false; // track if processed

                    if (ReadCell(i, j, k) == cell_attr.All)
                    {
                        // see if any neighbors are connected
                        int pass;
                        pass = 6; // check up to this many random directions
                        while (pass-- > 0)
                        {
                            switch (rand.Next(6))
                            {
                                case 0: // north?
                                    if (Legal(i, j - 1, k) && (ReadCell(i, j - 1, k) != cell_attr.All))
                                    {
                                        RemoveWalls(cell_attr.North, temp);
                                        added = true;
                                        pass = 0;
                                    }
                                    break;
                                case 1: // south?
                                    if (Legal(i, j + 1, k) && (ReadCell(i, j + 1, k) != cell_attr.All))
                                    {
                                        RemoveWalls(cell_attr.South, temp);
                                        added = true;
                                        pass = 0;
                                    }
                                    break;
                                case 2: // east?
                                    if (Legal(i + 1, j, k) && (ReadCell(i + 1, j, k) != cell_attr.All))
                                    {
                                        RemoveWalls(cell_attr.East, temp);
                                        added = true;
                                        pass = 0;
                                    }
                                    break;
                                case 4: // west?
                                    if (Legal(i - 1, j, k) && (ReadCell(i - 1, j, k) != cell_attr.All))
                                    {
                                        RemoveWalls(cell_attr.West, temp);
                                        added = true;
                                        pass = 0;
                                    }
                                    break;
                                case 5: // up?
                                    if (Legal(i, j, k + 1) && (ReadCell(i, j, k + 1) != cell_attr.All))
                                    {
                                        RemoveWalls(cell_attr.Up, temp);
                                        added = true;
                                        pass = 0;
                                    }
                                    break;
                                case 6: // down?
                                    if (Legal(i, j, k - 1) && (ReadCell(i, j, k - 1) != cell_attr.All))
                                    {
                                        RemoveWalls(cell_attr.Down, temp);
                                        added = true;
                                        pass = 0;
                                    }
                                    break;
                            }
                        }
                    }
                    if (!added)
                        unmarked.Enqueue(temp); // put back in place
                }
            } // while some left

            ProgressFunc("Finalizing           ");

            // external openings
            if (start_x == 0)
                RemoveWalls(cell_attr.West, start);
            if (start_x == SizeX - 1)
                RemoveWalls(cell_attr.East, start);
            if (start_y == 0)
                RemoveWalls(cell_attr.North, start);
            if (start_y == SizeY - 1)
                RemoveWalls(cell_attr.South, start);
            if (start_z == 0)
                RemoveWalls(cell_attr.Down, start);
            if (start_z == SizeZ - 1)
                RemoveWalls(cell_attr.Up, start);
            if (finish_x == 0)
                RemoveWalls(cell_attr.West, final);
            if (finish_x == SizeX - 1)
                RemoveWalls(cell_attr.East, final);
            if (finish_y == 0)
                RemoveWalls(cell_attr.North, final);
            if (finish_y == SizeY - 1)
                RemoveWalls(cell_attr.South, final);
            if (finish_z == 0)
                RemoveWalls(cell_attr.Down, final);
            if (finish_z == SizeZ - 1)
                RemoveWalls(cell_attr.Up, final);
        }

        /// <summary>
        ///     return bits set as follows: north = 1, east = 2, south = 4, east = 8, up = 16, down = 32
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public cell_attr CellWalls(int i, int j, int k = 0)
        {
            Debug.Assert(Legal(i, j, k));
            return ReadCell(i, j, k);
        }

        /// <summary>
        ///     Bit fields for each cell
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public uint CellData(int i, int j, int k = 0)
        {
            Debug.Assert(Legal(i, j, k));
            return (uint) (data[i, j, k]);
        }

        // set this with a function to cause progress strings to be passed back
        public void SetProgressFunction(Action<string> func)
        {
            ProgressFunc1 = func;
        }

        #endregion

        #region Implementation

        private int start_x, start_y, start_z;
        private int finish_x, finish_y, finish_z;
        private readonly int max_run; // Math.Max length of a straight run

        private Action<string> ProgressFunc1;

        private void ProgressFunc(string message, params object[] args)
        {
            var p = ProgressFunc1;
            if (p != null)
            {
                var msg = String.Format(message, args);
                p(msg);
            }
        }

        // directions deltas for each direction
        private static readonly int[] directions =
        {
            1, 0, 0,
            -1, 0, 0,
            0, 1, 0,
            0, -1, 0,
            0, 0, 1,
            0, 0, -1
        };


        // bit fields for set pieces
        [Flags]
        public enum cell_attr
        {
            None = 0,
            North = 1,
            East = 2,
            South = 4,
            West = 8,
            Up = 16,
            Down = 32,
            All = (1 + 2 + 4 + 8 + 16 + 32),
            Marked = 64
        };

        private ulong[,,] data; // used to track stuff for debugging, drawing, etc

        private class point3D
        {
            internal int x, y, z; // todo - used packed version to save memory

            public point3D(point3D p)
            {
                x = p.x;
                y = p.y;
                z = p.z;
            }

            public point3D(int x1, int y1, int z1)
            {
                x = x1;
                y = y1;
                z = z1;
            }

            public static bool operator ==(point3D a, point3D b)
            {
                // If both are null, or both are same instance, return true.
                if (ReferenceEquals(a, b))
                    return true;

                // If one is null, but not both, return false.
                if (((object) a == null) || ((object) b == null))
                    return false;

                // Return true if the fields match:
                return a.x == b.x && a.y == b.y && a.z == b.z;
            }

            public static bool operator !=(point3D a, point3D b)
            {
                return !(a == b);
            }

            public static point3D operator +(point3D a, point3D b)
            {
                return new point3D(a.x + b.x, a.y + b.y, a.z + b.z);
            }
        }

        // given a point, add adjacent cells marked with attribute all
        private void AddAdjacentUnused(int i, int j, int k, List<point3D> adjacent)
        {
            // add each direction if possible
            for (var pos = 0; pos < 3*6; pos += 3)
            {
                int dx, dy, dz;

                dx = directions[pos];
                dy = directions[pos + 1];
                dz = directions[pos + 2];

                if (Legal(i + dx, j + dy, k + dz) && (ReadCell(i + dx, j + dy, k + dz) == cell_attr.All))
                {
                    var pt = new point3D(i + dx, j + dy, k + dz);
                    adjacent.Add(pt);
                    AddToCell(cell_attr.Marked, pt); // in list
                }
            }
        }

        // AddAdjacentUnused

        private void SetCell(cell_attr attr, int i, int j, int k)
        {
            Debug.Assert(Legal(i, j, k));
            Cells[i, j, k].Attribute = attr;
        }

        // SetCell

        private cell_attr ReadCell(int i, int j, int k)
        {
            Debug.Assert(Legal(i, j, k));
            return Cells[i, j, k].Attribute;
        }

        // fills in cells, makes hollow box with outside set
        private void InitCells()
        {
            // fills in cells, adding all walls in each cell
            Cells = new Cell[SizeX, SizeY, SizeZ];
            data = new ulong[SizeX, SizeY, SizeZ];

            for (var i = 0; i < SizeX; i++)
                for (var j = 0; j < SizeY; j++)
                    for (var k = 0; k < SizeZ; k++)
                    {
                        Cells[i, j, k] = new Cell {Attribute = cell_attr.All};
                    }
        }

        private void RemoveFromCell(cell_attr attr, point3D coord)
        {
            if (!Legal(coord.x, coord.y, coord.z))
                return;
            var temp = ReadCell(coord.x, coord.y, coord.z);
            temp = (cell_attr) ((uint) temp & ~(uint) attr);
            SetCell(temp, coord.x, coord.y, coord.z);
        }


        private void AddToCell(cell_attr attr, point3D coord)
        {
            var temp = ReadCell(coord.x, coord.y, coord.z);
            temp = (cell_attr) ((uint) temp | (uint) attr);
            SetCell(temp, coord.x, coord.y, coord.z);
        }


        // add deadend in maze, and return true iff one found/added at start point
        private bool AddDeadEnd(int sx, int sy, int sz, List<point3D> found_coords)
        {
            // add dead end in maze, and return true iff one found/added at start point
            // add new cells (i,j,k) to found coords
            var dir_found = false;
            var pass = 0;
            int dx, dy, dz, length = 0; // describes the dead end
            dx = dy = dz = 0;

            while ((dir_found == false) && (pass++ < 40))
            {
                switch (rand.Next(6))
                {
                    case 0: // dead end north
                        if ((sy > 0) && (ReadCell(sx, sy - 1, sz) == cell_attr.All))
                        {
                            // we have ok cells to try from
                            dir_found = true;
                            dx = 0;
                            dy = -1;
                            dz = 0;
                            length = rand.Next(sy);
                        }
                        break;
                    case 1: // dead end south
                        if ((sy < SizeY - 1) && (ReadCell(sx, sy + 1, sz) == cell_attr.All))
                        {
                            // we have ok cells to try from
                            dir_found = true;
                            dx = 0;
                            dy = 1;
                            dz = 0;
                            length = rand.Next(Math.Abs(SizeY - sy));
                        }
                        break;
                    case 2: // dead end west
                        if ((sx > 0) && (ReadCell(sx - 1, sy, sz) == cell_attr.All))
                        {
                            // we have ok cells to try from
                            dir_found = true;
                            dx = -1;
                            dy = 0;
                            dz = 0;
                            length = rand.Next(sx);
                        }
                        break;
                    case 3: // dead end east
                        if ((sx < SizeX - 1) && (ReadCell(sx + 1, sy, sz) == cell_attr.All))
                        {
                            // we have ok cells to try from
                            dir_found = true;
                            dx = 1;
                            dy = 0;
                            dz = 0;
                            length = rand.Next(Math.Abs(sx - SizeX));
                        }
                        break;
                    case 4: // dead end down
                        if ((sz > 0) && (ReadCell(sx, sy, sz - 1) == cell_attr.All))
                        {
                            // we have ok cells to try from
                            dir_found = true;
                            dx = 0;
                            dy = 0;
                            dz = -1;
                            length = rand.Next(sz);
                        }
                        break;
                    case 5: // dead end up
                        if ((sz < SizeZ - 1) && (ReadCell(sx, sy, sz + 1) == cell_attr.All))
                        {
                            // we have ok cells to try from
                            dir_found = true;
                            dx = 0;
                            dy = 0;
                            dz = 1;
                            length = rand.Next(Math.Abs(sz - SizeZ));
                        }
                        break;
                }
            }

            if (dir_found == false)
                return false; // none found

            length %= max_run;
            length++;

            // do the dead end
            while (length-- > 0)
            {
                if (Legal(sx + dx, sy + dy, sz + dz) && (ReadCell(sx + dx, sy + dy, sz + dz) == cell_attr.All))
                {
                    var ss = new point3D(sx, sy, sz);
                    if (dy < 0) // north
                    {
                        RemoveWalls(cell_attr.North, ss);
                        sy--;
                        found_coords.Add(new point3D(sx, sy, sz));
                    }
                    else if (dy > 0) // south
                    {
                        RemoveWalls(cell_attr.South, ss);
                        sy++;
                        found_coords.Add(new point3D(sx, sy, sz));
                    }
                    else if (dx < 0) // west
                    {
                        RemoveWalls(cell_attr.West, ss);
                        sx--;
                        found_coords.Add(new point3D(sx, sy, sz));
                    }
                    else if (dx > 0) // east
                    {
                        RemoveWalls(cell_attr.East, ss);
                        sx++;
                        found_coords.Add(new point3D(sx, sy, sz));
                    }
                    else if (dz < 0) // down
                    {
                        RemoveWalls(cell_attr.Down, ss);
                        sz--;
                        found_coords.Add(new point3D(sx, sy, sz));
                    }
                    else if (dz > 0) // up
                    {
                        RemoveWalls(cell_attr.Up, ss);
                        sz++;
                        found_coords.Add(new point3D(sx, sy, sz));
                    }
                }
                else
                    length = 0; // we hit another, so end now
            } // while count

            return true;
        }

        // AddDeadEnd

        // add perturbation in maze, and return true iff one found/added at start point
        private bool AddPertubation(point3D pt, int min_run, int max_run, List<point3D> found_coords)
        {
            int sx = pt.x, sy = pt.y, sz = pt.z;
            // given a start point, add a perturbation in the maze, return true iff one found and added
            bool nb_found; // look for neighbor
            var pass = 0; // prevent infinite loop
            nb_found = false;
            var perturbed = false; // set this when perturbed at least one cell

            var count = 0; // perturbation this long
            int dx1, dy1, dz1; // move like this until count reached, then use dx2,dy2, then count back
            int dx2, dy2, dz2; // move like this at end of movement
            dx1 = dy1 = dz1 = 0;
            dx2 = dy2 = dz2 = 0;

            while ((nb_found == false) && (pass++ < 40))
            {
                switch (rand.Next(6))
                {
                        // todo - need more variation here - for example up/down always moves +y direction at end...
                    case 0: // perturb north
                        if ((sx < SizeX - 1) && (ReadCell(sx, sy, sz) == NotAttr(cell_attr.East | cell_attr.West)) &&
                            (ReadCell(sx + 1, sy, sz) == NotAttr(cell_attr.East | cell_attr.West)) && (sy != 0))
                        {
                            // we have ok cells to try from
                            nb_found = true;
                            dx1 = 0;
                            dy1 = -1;
                            dz1 = 0;
                            dx2 = 1;
                            dy2 = 0;
                            dz2 = 0;
                            count = rand.Next(sy);
                        }
                        break;
                    case 1: // perturb south
                        if ((sx < SizeX - 1) && (ReadCell(sx, sy, sz) == NotAttr(cell_attr.East | cell_attr.West)) &&
                            (ReadCell(sx + 1, sy, sz) == NotAttr(cell_attr.East | cell_attr.West)) && ((sy != SizeY)))
                        {
                            // we have ok cells to try from
                            nb_found = true;
                            dx1 = 0;
                            dy1 = 1;
                            dz1 = 0;
                            dx2 = 1;
                            dy2 = 0;
                            dz2 = 0;
                            count = rand.Next(Math.Abs(SizeY - sy));
                        }
                        break;
                    case 2: // perturb west
                        if ((sy < SizeY - 1) && (ReadCell(sx, sy, sz) == NotAttr(cell_attr.North | cell_attr.South)) &&
                            (ReadCell(sx, sy + 1, sz) == NotAttr(cell_attr.North | cell_attr.South)) && (sx != 0))
                        {
                            // we have ok cells to try from
                            nb_found = true;
                            dx1 = -1;
                            dy1 = 0;
                            dz1 = 0;
                            dx2 = 0;
                            dy2 = 1;
                            dz2 = 0;
                            count = rand.Next(sx);
                        }
                        break;
                    case 3: // perturb east
                        if ((sy < SizeY - 1) && (ReadCell(sx, sy, sz) == NotAttr(cell_attr.North | cell_attr.South)) &&
                            (ReadCell(sx, sy + 1, sz) == NotAttr(cell_attr.North | cell_attr.South)) && ((sx != SizeX)))
                        {
                            // we have ok cells to try from
                            nb_found = true;
                            dx1 = 1;
                            dy1 = 0;
                            dz1 = 0;
                            dx2 = 0;
                            dy2 = 1;
                            dz2 = 0;
                            count = rand.Next(Math.Abs(sx - SizeX));
                        }
                        break;
                    case 4: // perturb up
                        if ((sy < SizeY - 1) && (ReadCell(sx, sy, sz) == NotAttr(cell_attr.North | cell_attr.South)) &&
                            (ReadCell(sx, sy + 1, sz) == NotAttr(cell_attr.North | cell_attr.South)) && (sz != 0))
                        {
                            // we have ok cells to try from
                            nb_found = true;
                            dx1 = 0;
                            dy1 = 0;
                            dz1 = 1;
                            dx2 = 0;
                            dy2 = 1;
                            dz2 = 0;
                            count = rand.Next(sz);
                        }
                        break;
                    case 5: // perturb down
                        if ((sy < SizeY - 1) && (ReadCell(sx, sy, sz) == NotAttr(cell_attr.North | cell_attr.South)) &&
                            (ReadCell(sx, sy + 1, sz) == NotAttr(cell_attr.North | cell_attr.South)) && ((sz != SizeZ)))
                        {
                            // we have ok cells to try from
                            nb_found = true;
                            dx1 = 0;
                            dy1 = 0;
                            dz1 = -1;
                            dx2 = 0;
                            dy2 = 1;
                            dz2 = 0;
                            count = rand.Next(Math.Abs(sz - SizeZ));
                        }
                        break;
                }
            }

            if (nb_found == false)
                return false; // none found

            // do the perturbation
            count %= max_run;
            count++;
            if (count < min_run) count = min_run;

            while (count-- > 0)
            {
                if (Legal(sx + dx1, sy + dy1, sz + dz1) && Legal(sx + dx1 + dx2, sy + dy1 + dy2, sz + dz1 + dz2))
                    if ((ReadCell(sx + dx1, sy + dy1, sz + dz1) == cell_attr.All) &&
                        (ReadCell(sx + dx1 + dx2, sy + dy1 + dy2, sz + dz1 + dz2) == cell_attr.All))
                    {
                        // ensure all cells ok
                        if (perturbed == false)
                        {
                            var ss1 = new point3D(sx, sy, sz);
                            // first pass, so block old passage
                            if ((dy2 < 0) && (sy > 0)) // north
                                AddWalls(cell_attr.North, ss1);
                            else if ((dy2 > 0) && (sy < SizeY)) // south
                                AddWalls(cell_attr.South, ss1);
                            else if ((dx2 < 0) && (sx > 0)) // west
                                AddWalls(cell_attr.West, ss1);
                            else if ((dx2 > 0) && (sx < SizeX)) // east
                                AddWalls(cell_attr.East, ss1);
                            // todo - dz cases?
                        }
                        perturbed = true; // we added at least one

                        var ss = new point3D(sx, sy, sz);
                        var sd = new point3D(sx + dx2, sy + dy2, sz + dz2);

                        if ((dy1 < 0) & (sy > 0)) // north
                        {
                            RemoveWalls(cell_attr.North, ss);
                            RemoveWalls(cell_attr.North, sd);
                            sy--;
                            found_coords.Add(new point3D(sx, sy, sz));
                            found_coords.Add(new point3D(sx + dx2, sy + dy2, sz + dz2));
                        }
                        else if ((dy1 > 0) && (sy < SizeY - 1)) // south
                        {
                            RemoveWalls(cell_attr.South, ss);
                            RemoveWalls(cell_attr.South, sd);
                            sy++;
                            found_coords.Add(new point3D(sx, sy, sz));
                            found_coords.Add(new point3D(sx + dx2, sy + dy2, sz + dz2));
                        }
                        else if ((dx1 < 0) && (sx > 0)) // west
                        {
                            RemoveWalls(cell_attr.West, ss);
                            RemoveWalls(cell_attr.West, sd);
                            sx--;
                            found_coords.Add(new point3D(sx, sy, sz));
                            found_coords.Add(new point3D(sx + dx2, sy + dy2, sz + dz2));
                        }
                        else if ((dx1 > 0) && (sx < SizeX - 1)) // east
                        {
                            RemoveWalls(cell_attr.East, ss);
                            RemoveWalls(cell_attr.East, sd);
                            sx++;
                            found_coords.Add(new point3D(sx, sy, sz));
                            found_coords.Add(new point3D(sx + dx2, sy + dy2, sz + dz2));
                        }
                        else if ((dz1 < 0) && (sz > 0)) // down
                        {
                            RemoveWalls(cell_attr.Down, ss);
                            RemoveWalls(cell_attr.Down, sd);
                            sz--;
                            found_coords.Add(new point3D(sx, sy, sz));
                            found_coords.Add(new point3D(sx + dx2, sy + dy2, sz + dz2));
                        }
                        else if ((dz1 > 0) && (sz < SizeZ - 1)) // up
                        {
                            RemoveWalls(cell_attr.Up, ss);
                            RemoveWalls(cell_attr.Up, sd);
                            sz++;
                            found_coords.Add(new point3D(sx, sy, sz));
                            found_coords.Add(new point3D(sx + dx2, sy + dy2, sz + dz2));
                        }
                    }
                    else
                        count = 0; // we hit another, so end now
            } // while count

            // do connection between two corridors added
            if (dx2 > 0)
                RemoveWalls(cell_attr.East, new point3D(sx, sy, sz));
            else if (dx2 < 0)
                RemoveWalls(cell_attr.West, new point3D(sx, sy, sz));
            else if (dy2 > 0)
                RemoveWalls(cell_attr.South, new point3D(sx, sy, sz));
            else if (dy2 < 0)
                RemoveWalls(cell_attr.North, new point3D(sx, sy, sz));
            // todo - dz2 cases?

            return true;
        }

        private cell_attr Opposite(cell_attr dir)
        {
            switch (dir)
            {
                case cell_attr.East:
                    return cell_attr.West;
                case cell_attr.West:
                    return cell_attr.East;
                case cell_attr.North:
                    return cell_attr.South;
                case cell_attr.South:
                    return cell_attr.North;
                case cell_attr.Up:
                    return cell_attr.Down;
                case cell_attr.Down:
                    return cell_attr.Up;
                default:
                    throw new Exception("Not a valid direction " + dir);
            }
        }

        private point3D Direction(cell_attr dir)
        {
            int dx = 0, dy = 0, dz = 0;
            switch (dir)
            {
                case cell_attr.East:
                    dx = 1;
                    break;
                case cell_attr.West:
                    dx = -1;
                    break;
                case cell_attr.North:
                    dy = -1;
                    break;
                case cell_attr.South:
                    dy = 1;
                    break;
                case cell_attr.Up:
                    dz = 1;
                    break;
                case cell_attr.Down:
                    dz = -1;
                    break;
                default:
                    throw new Exception("Not a valid direction " + dir);
            }
            return new point3D(dx, dy, dz);
        }

        // remove given walls, both cells needing modified are fixed
        private void RemoveWalls(cell_attr attr, point3D coord)
        {
            Action<cell_attr> act = dir =>
            {
                if ((int) (attr & dir) != 0)
                {
                    var delta = Direction(dir);
                    RemoveFromCell(dir, coord);
                    RemoveFromCell(Opposite(dir), coord + delta);
                }
            };

            act(cell_attr.East);
            act(cell_attr.West);
            act(cell_attr.North);
            act(cell_attr.South);
            act(cell_attr.Up);
            act(cell_attr.Down);

            Debug.Assert((ReadCell(coord.x, coord.y, coord.z) & attr) == 0);
        }

        // add given walls, both cells needing modified are fixed
        private void AddWalls(cell_attr attr, point3D coord)
        {
            Action<cell_attr> act = dir =>
            {
                if ((int) (attr & dir) != 0)
                {
                    var delta = Direction(dir);
                    AddToCell(dir, coord);
                    AddToCell(Opposite(dir), coord + delta);
                }
            };

            act(cell_attr.East);
            act(cell_attr.West);
            act(cell_attr.North);
            act(cell_attr.South);
            act(cell_attr.Up);
            act(cell_attr.Down);
        }

#if false
    // create 2D or 3D maze, given start, finish coords in range [0 - (sx-1)], etc
    // create 2D or 3D maze, given start, finish coords in range [0 - (sx-1)], etc
    // uses new method
        private void Create2(int start_x1, int finish_x1, int size_x1, int start_y1, int finish_y1, int size_y1,
                             int start_z1, int finish_z1, int size_z1)
        {
            // todo - check inputs
            start_x = start_x1;
            start_y = start_y1;
            start_z = start_z1;
            finish_x = finish_x1;
            finish_y = finish_y1;
            finish_z = finish_z1;
            SizeX = size_x1;
            SizeY = size_y1;
            SizeZ = size_z1;
            InitCells();

            uint text_out = 0; // counter of when to make messages

            // algorithm:
            //   0. Make external openings for start
            //   1. Mark start and end as used, add adjacent cells to a list
            //   2. While not all squares in maze, pick one in adjacent list randomly, add it
            //   3. Make external openings for finish

            // step 0 - external openings
            if (start_x == 0)
                RemoveWalls(cell_attr.West, start_x, start_y, start_z);
            if (start_x == SizeX - 1)
                RemoveWalls(cell_attr.East, start_x, start_y, start_z);
            if (start_y == 0)
                RemoveWalls(cell_attr.North, start_x, start_y, start_z);
            if (start_y == SizeY - 1)
                RemoveWalls(cell_attr.South, start_x, start_y, start_z);
            if (start_z == 0)
                RemoveWalls(cell_attr.Down, start_x, start_y, start_z);
            if (start_z == SizeZ - 1)
                RemoveWalls(cell_attr.Up, start_x, start_y, start_z);


            // step 1
            int added_goal = SizeX*SizeY*SizeZ, added_count;

            AddToCell(cell_attr.Marked, start_x, start_y, start_z); // mark used
            //	AddToCell(marked,finish_x,finish_y,finish_z); // mark used
            //	added_count = 2;
            added_count = 2;
            List<point3D> adjacent = new List<point3D>(); // cells adjacent to those used

            AddAdjacentUnused(start_x, start_y, start_z, adjacent);
            //	AddAdjacentUnused(finish_x, finish_y, finish_z, adjacent);

            // step 2
            while (added_count < added_goal)
            {
                // todo - make a random function returning a 32 bit number, rand_max is ONLY about 32768 !
                int i, j, k; // positions
                int pos;
                point3D pt;
                pos = rand.Next(adjacent.Count);

                //List<point3D>::iterator iter;
                //iter = adjacent.begin();
                //advance(iter,pos);
                pt = adjacent[pos]; // *iter;
                i = pt.x;
                j = pt.y;
                k = pt.z;

                // see if any directions unused, if so, mark, join, and add
                // see if any neighbors are connected
                int pass;
                bool found;
                found = false;
                pass = 36; // check up to this many random directions
                while ((false == found) && (pass-- > 0))
                {
                    // todo - remove switch, use table driven mode - "directions" defined above...
                    switch (rand.Next(6))
                    {
                        case 0: // north?
                            if (Legal(i, j - 1, k) && ((ReadCell(i, j - 1, k) & cell_attr.All) != cell_attr.All))
                            {
                                RemoveWalls(cell_attr.North, i, j, k);
                                found = true;
                            }
                            break;
                        case 1: // south?
                            if (Legal(i, j + 1, k) && ((ReadCell(i, j + 1, k) & cell_attr.All) != cell_attr.All))
                            {
                                RemoveWalls(cell_attr.South, i, j, k);
                                found = true;
                            }
                            break;
                        case 2: // east?
                            if (Legal(i + 1, j, k) && ((ReadCell(i + 1, j, k) & cell_attr.All) != cell_attr.All))
                            {
                                RemoveWalls(cell_attr.East, i, j, k);
                                found = true;
                            }
                            break;
                        case 4: // west?
                            if (Legal(i - 1, j, k) && ((ReadCell(i - 1, j, k) & cell_attr.All) != cell_attr.All))
                            {
                                RemoveWalls(cell_attr.West, i, j, k);
                                found = true;
                            }
                            break;
                        case 5: // up?
                            if (Legal(i, j, k + 1) && ((ReadCell(i, j, k + 1) & cell_attr.All) != cell_attr.All))
                            {
                                RemoveWalls(cell_attr.Up, i, j, k);
                                found = true;
                            }
                            break;
                        case 6: // down?
                            if (Legal(i, j, k - 1) && ((ReadCell(i, j, k - 1) & cell_attr.All) != cell_attr.All))
                            {
                                RemoveWalls(cell_attr.Down, i, j, k);
                                found = true;
                            }
                            break;
                    }
                } // while adding passes
                if (true == found)
                {
                    adjacent.RemoveAt(pos);
                    AddAdjacentUnused(i, j, k, adjacent);
                    added_count++;
                }
            } // while added < goal

            ProgressFunc("Finalizing           ");

            // step 3
            if (finish_x == 0)
                RemoveWalls(cell_attr.West, finish_x, finish_y, finish_z);
            if (finish_x == SizeX - 1)
                RemoveWalls(cell_attr.East, finish_x, finish_y, finish_z);
            if (finish_y == 0)
                RemoveWalls(cell_attr.North, finish_x, finish_y, finish_z);
            if (finish_y == SizeY - 1)
                RemoveWalls(cell_attr.South, finish_x, finish_y, finish_z);
            if (finish_z == 0)
                RemoveWalls(cell_attr.Down, finish_x, finish_y, finish_z);
            if (finish_z == SizeZ - 1)
                RemoveWalls(cell_attr.Up, finish_x, finish_y, finish_z);

        }
#endif

        // is coord legal?
        private bool Legal(int i, int j, int k)
        {
            // is coord legal?
            if ((i < 0) || (j < 0) || (k < 0))
                return false;
            if ((i >= SizeX) || (j >= SizeY) || (k >= SizeZ))
                return false;
            return true;
        }

        // return all attributes except the specified ones
        private cell_attr NotAttr(cell_attr attr)
        {
            return (~attr) & cell_attr.All;
        }


        // source of randomness for the maze generation
        private readonly Random rand = new Random();

        #endregion
    }
}