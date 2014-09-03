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
using System.Threading;
using System.Threading.Tasks;
using Hypnocube.Demo.ViewModel;
using Hypnocube.MVVM;

namespace Hypnocube.Demo.Model
{
    /// <summary>
    ///     Manages asynchronous image generation and playback
    /// </summary>
    public sealed class Playback : NotifiableBase
    {
        private readonly Action<uint[]> DrawFrameToScreen;
        private readonly DemoManager demoManager;
        private readonly DemoControlViewModel model;
        private volatile bool looping;


        /// <summary>
        ///     Gamma correction table, for each byte 0-255, this is what should be
        ///     output from the device
        /// </summary>
        private byte[] gammaTable;

        private Task task;

        public Playback(DemoControlViewModel model, DemoManager demoManager, Action<uint[]> drawFrameToScreen)
        {
            this.model = model;
            this.demoManager = demoManager;
            DrawFrameToScreen = drawFrameToScreen;
            CreateGammaTable(2.2);
        }

        private void CreateGammaTable(double gamma)
        {
            var t = new byte[256];
            var g = gamma;
            if (g <= 0) return;
            for (var b = 0; b < 256; ++b)
            {
                var v = (int) (Math.Round(Math.Pow(b/255.0, g)*255.0));
                if (v < 0) v = 0;
                if (v > 255) v = 255;
                t[b] = (byte) v;
            }
            gammaTable = t; // copy out
        }


        public void Start()
        {
            looping = true;
            if (task == null)
                task = Task.Factory.StartNew(Loop);
        }

        public void Stop()
        {
            looping = false;
        }


        private void Loop()
        {
            var gamma = 1.0;
            while (true)
            {
                while (!looping)
                {
                    // do nothing
                    Thread.Sleep(10);
                }

                var lastTime = Environment.TickCount;
                var frameIndex = 0;

                var selIndex = model.SelectedDemoIndex;

                while (looping)
                {
                    try
                    {
                        var deltaTime = (int) (1000.0/model.FramesPerSecond + 0.5);
                        var currentTime = Environment.TickCount;
                        if (currentTime > lastTime + deltaTime)
                        {
                            lastTime += deltaTime;
                            var strands = model.Strands;
                            if (selIndex != model.SelectedDemoIndex)
                                frameIndex = 0;
                            selIndex = model.SelectedDemoIndex;
                            if (selIndex < 0)
                                selIndex = 0;
                            var useGamma = model.UseGammaCorrection;

                            if (model.GammaCorrection != gamma)
                            {
                                gamma = model.GammaCorrection;
                                CreateGammaTable(gamma);
                            }

                            if (1 <= strands && strands <= 16)
                                demoManager.Remapper.Strands = strands;
                            var frame = demoManager.NextFrame(selIndex, gammaTable, useGamma);

                            frameIndex++;
                            if (frameIndex > demoManager.VisualizationSchedule[selIndex].FrameLength)
                            {
                                // next one
                                var max = demoManager.VisualizationSchedule.Count;
                                if (max > 0)
                                    model.SelectedDemoIndex = (model.SelectedDemoIndex + 1)%max;
                                frameIndex = 0;
                            }

                            DrawFrameToScreen(frame);
                        }
                    }
                    catch (Exception ex)
                    {
                        model.Messager.AddError("Exception " + ex.ToString());
                        looping = false;
                    }
                }
            }
        }
    }
}