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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Hypnocube.Demo.Model.Demos;
using Hypnocube.Demo.Model.Remappings;
using Hypnocube.Device;
using Hypnocube.MVVM;

namespace Hypnocube.Demo.Model
{
    public class DemoManager
    {
        private const int BlendLength = 30; // frames to alpha blend

        /// <summary>
        ///     Running and paused visualizations, one for each visualization type
        /// </summary>
        private readonly List<DemoBase> liveVisualizations = new List<DemoBase>();

        private int blendFrame;
        private Action<uint[]> imageFilter;

        private int lastIndex = -1;
        private uint[] tempImage;

        public DemoManager()
        {
            //Width = 600;
            //Height = 1;

            //Width = Height = 100;

            Brightness = 100;

            liveVisualizations = new List<DemoBase>();
            VisualizationTypes = new ObservableCollection<Type>();
            VisualizationSchedule = new ObservableCollection<VisualizationWrapper>();

            Remappers = new List<Remapper>();
            ReflectVisualization();
            ReflectRemappers();
        }

        public Remapper Remapper { get; private set; }
        public List<Remapper> Remappers { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }


        /// <summary>
        /// Type for each visualization in the system
        /// </summary>
        public ObservableCollection<Type> VisualizationTypes { get; private set; }


        /// <summary>
        /// Schedule of visualizations and frames to play
        /// </summary>
        public ObservableCollection<VisualizationWrapper> VisualizationSchedule { get; private set; }

        public double Brightness { get; set; }

        private void ReflectVisualization()
        {
            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.IsSubclassOf(typeof (DemoBase))
                select t;
            foreach (var type in q)
                VisualizationTypes.Add(type);
        }

        private void ReflectRemappers()
        {
            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.IsSubclassOf(typeof (Remapper))
                select t;
            foreach (var type in q)
                Remappers.Add(Activator.CreateInstance(type) as Remapper);
        }

        private HypnoLsdController device;
        public void Start(HypnoLsdController device, Remapper remapper, int mbaud, Action<uint[]> imageFilterIn = null)
        {
            this.device = device;

            Remapper = remapper;
            Width = remapper.Width;
            Height = remapper.Height;

            if (imageFilterIn != null)
                this.imageFilter = imageFilterIn;
            else
                this.imageFilter = b => { }; // identity

            AddDemos();
            
            // get attention
            device.WriteString("\r\n");
            Thread.Sleep(250);

            //device.ConnectionSpeed = 1000000; // 1 Mbaud for testing
            //Thread.Sleep(100);

            //Thread.Sleep(1000);
            //device.Drawing = true;
            //Thread.Sleep(1000);
            //device.Drawing = false;
            //Thread.Sleep(1000);

            device.WriteString("\r\n");


            device.ConnectionSpeed = mbaud; // 12000000;// 3000000;// 12000000; // 12 Mbaud for testing
            Thread.Sleep(500);

            const int delay = 200;
            device.Drawing = false;
            Thread.Sleep(delay);

            device.Drawing = true;
            Thread.Sleep(delay);
            device.Drawing = false;
            Thread.Sleep(delay);

            if (remapper.SupportedStrands.Contains(remapper.Strands))
            {
                device.SetSize(remapper.Strands, remapper.Width * remapper.Height / remapper.Strands);
            }
            else
            {
                throw new NotImplementedException("Remapper does not support " + remapper.Strands + " strands");
            }

            // some context switches to ensure we're at the top in case we disconnected early last time.
            device.Drawing = true;
            Thread.Sleep(delay);
        }

        /// <summary>
        /// Get the example demo of the given type
        /// 
        /// </summary>
        /// <param name="demoIndex"></param>
        /// <returns></returns>
        private DemoBase GetDemoFromIndex(int demoIndex)
        {
            return liveVisualizations[demoIndex];
            //var demoType = VisualizationSchedule[demoIndex].DemoType;

            //foreach (var d in liveVisualizations)
            //    if (demoType == d.GetType())
            //    {
            //        d.ParameterText = VisualizationSchedule[demoIndex].ParameterText;
            //        return d;
            //    }
            //throw new Exception(String.Format("Unknown demo type {0}", demoType));
        }

        private void AddDemos()
        {
            liveVisualizations.Clear();
            foreach (var vs in VisualizationSchedule)
            {
                var type = vs.DemoType;
                var demo = Activator.CreateInstance(type, new object[] {Width, Height}) as DemoBase;
                if (demo != null)
                {
                    demo.Reset();
                    demo.ParameterText = vs.ParameterText;
                    liveVisualizations.Add(demo);
                }
            }
        }


        /// <summary>
        /// Apply brightness and any other things to the image
        /// Ok to modify image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="gamma"></param>
        /// <param name="useGamma"></param>
        /// <returns></returns>
        private void PostProcessImage(uint[] image, byte[] gamma, bool useGamma)
        {
            if (image == null) return;

            ApplyBrightness(image);
            imageFilter(image); // any external filtering

            if (device.IsConnected)
                device.WriteImage(Remapper.MapImage(image, useGamma ? gamma : null));
        }

        private void ApplyBrightness(uint[] img)
        {
            for (var i = 0; i < img.Length; ++i)
            {
                var p = img[i]; // BGRA
                var r = (uint) (((p >> 16) & 255)*Brightness/100);
                var g = (uint) (((p >> 8) & 255)*Brightness/100);
                var b = (uint) (((p >> 0) & 255)*Brightness/100);
                img[i] = (b << 0) + (g << 8) + (r << 16) + 0xff000000;
            }
        }

        /// <summary>
        ///     Draw the next frame of the current visualization
        /// </summary>
        public uint[] NextFrame(int type, byte[] gammaCorrection, bool useGamma)
        {
            if (VisualizationSchedule.Count == 0)
                return null;
            var demoIndex = type%VisualizationSchedule.Count;
            if (lastIndex == -1)
                lastIndex = demoIndex;

            if (lastIndex != demoIndex && blendFrame == 0)
            {
                // start blend
                blendFrame = BlendLength; // countdown
                var demo = GetDemoFromIndex(demoIndex);
                demo.ParameterText = VisualizationSchedule[demoIndex].ParameterText;
                demo.Reset();
            }

            var len = Width*Height;

            // make sure we have a good temp image for workspace
            if (tempImage == null || tempImage.Length != len)
                tempImage = new uint[len];

            if (lastIndex != demoIndex && blendFrame != 0)
            {
                // do blend
                blendFrame--;
                if (blendFrame == 0)
                {
                    // switch over
                    lastIndex = demoIndex;
                }

                var demoOld = GetDemoFromIndex(lastIndex);
                demoOld.Update();
                var imageOld = demoOld.Image;

                var demoNew = GetDemoFromIndex(demoIndex);
                demoNew.Update();
                var imageNew = demoNew.Image;

                BlendFrames(tempImage, imageOld, imageNew, (double) blendFrame/BlendLength);
            }
            else
            {
                // normal
                var demo = GetDemoFromIndex(demoIndex);
                demo.Update();
                var image = demo.Image;
                Array.Copy(image, tempImage, image.Length);
            }

            PostProcessImage(tempImage, gammaCorrection, useGamma);

            // black check
            var allBlack = true;
            for (var i = 0; i < tempImage.Length && allBlack; ++i)
                allBlack &= (tempImage[i] & 0xFF000000) == 0;

            if (allBlack)
                Debug.WriteLine("Frame black");


            return tempImage;
        }

        private static void BlendFrames(IList<uint> dstImage, IList<uint> imageOld, IList<uint> imageNew, double alpha)
        {
            for (var i = 0; i < imageOld.Count; ++i)
            {
                var p1 = imageOld[i]; // BGRA
                var r1 = (p1 >> 16) & 255;
                var g1 = (p1 >> 8) & 255;
                var b1 = (p1 >> 0) & 255;
                var p2 = imageNew[i]; // BGRA
                var r2 = (p2 >> 16) & 255;
                var g2 = (p2 >> 8) & 255;
                var b2 = (p2 >> 0) & 255;

                var r3 = (uint) (r1*alpha + r2*(1 - alpha));
                var g3 = (uint) (g1*alpha + g2*(1 - alpha));
                var b3 = (uint) (b1*alpha + b2*(1 - alpha));

                dstImage[i] = (r3 << 16) + (g3 << 8) + b3 + 0xFF000000;
            }
        }

        #region Nested type: VisualizationWrapper

        public class VisualizationWrapper : NotifiableBase
        {
            #region FrameLength Property

            private int frameLength = 300;

            /// <summary>
            ///     Gets or sets the number of frames to render.
            /// </summary>
            public int FrameLength
            {
                get { return frameLength; }
                set
                {
                    // return true if there was a change.
                    SetField(ref frameLength, value);
                }
            }

            #endregion

            #region ParameterText Property
            private string parameterText = "";
            /// <summary>
            /// Gets or sets the demo parameter text.
            /// </summary>
            public string ParameterText
            {
                get { return parameterText; }
                set
                {
                    // return true if there was a change.
                    SetField(ref parameterText, value);
                }
            }
            #endregion

            #region ParameterDescription Property
            private string parameterDescription = "No parameters defined";
            /// <summary>
            /// Gets or sets the parameter help.
            /// </summary>
            public string ParameterDescription
            {
                get { return parameterDescription; }
                set
                {
                    // return true if there was a change.
                    SetField(ref parameterDescription, value);
                }
            }
            #endregion



            public VisualizationWrapper(Type demoType, int frameLength)
            {
                DemoType = demoType;
                FrameLength = frameLength;

                // get parameter text and help
                var demo = Activator.CreateInstance(demoType, new object[] {133, 133}) as DemoBase;
                if (demo != null)
                {
                    demo.Reset();
                    ParameterText = demo.ParameterText;
                    ParameterDescription = demo.ParameterDescription;

                }
            }

            public Type DemoType { get; private set; }
            public override string ToString()
            {
                return DemoType.Name + " " + FrameLength;
            }
        }

        #endregion
    }
}