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

namespace Hypnocube.Demo.Model.Remappings
{
    internal class Cylinder3X2 : Panels
    {
        private const int PanelX = 50;
        private const int PanelY = 25;

        private readonly PanelBox[] panels =
        {
            new PanelBox(0, 0, PanelX - 1, PanelY - 1, true, true),
            new PanelBox(PanelX, 0, 2*PanelX - 1, PanelY - 1, true, true),
            new PanelBox(2*PanelX, 0, 3*PanelX - 1, PanelY - 1, true, true),
            new PanelBox(0, PanelY, PanelX - 1, 2*PanelY - 1, true, true),
            new PanelBox(PanelX, PanelY, 2*PanelX - 1, 2*PanelY - 1, true, true),
            new PanelBox(2*PanelX, PanelY, 3*PanelX - 1, 2*PanelY - 1, true, true)
        };

        public Cylinder3X2()
        {
            //Width = 200;
            //Height = 25;
            //Strands = 1;

            Width = 150;
            Height = 50;
            Strands = 6;
            CreateMapping(panels);
        }


    }
}