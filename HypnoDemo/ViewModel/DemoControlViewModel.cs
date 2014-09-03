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
/* TODO
 *  1. Finish HSL color stretching to make symmetric and C-inf and tunable, then check visually
 *  2. Allow 1/4 and 1/9 spacing for pixel grid to more accurately mimic output
 *  3. Demo:
 *     **- new plasma, split old
 *     - polygon shaded, better
 *     - rasterbar better
 *     - raytracer faster
 *     - make old scrolly better
 *     - make new scrolly better
 *     - vector balls better, more shapes, animation, movement
 *     - wormhole better, centered, colors?
 *     - finish mandelbrot
 *  4. Add demos
 *     - Fish
 *     **- Chemicals
 *     - Lemmings?
 *     - sliding colored blocks, orthogonal
 *     - space images, ala hubble
 *     - starfield
 *     - tunnel zoomer
 *     - matrix style glyph droppings, code falling, windings
 *     **- world flags
 *     - Nyan cat with rainbows, meme
 *     - rainbows
 *     - tiling
 *     - hilbert curve filler
 *     - little walking creatures?
 *     **- rotozoomer bitmaps
 *     - image scroller, paintings
 *     - famous video
 *     - text credits, specs, ad at end
 *     - fade to black
 *     **- random noise one
 *     - old school blinking computer one
 *     **- Large rain drops, falling onscreen, making wobbles, perhaps as a filter
 *  5. Add over the top effects, like shapes that slide over and add filters:
 *     - grayscale
 *     - hue cycle
 *     - sepia
 *     - RGB swaps
 *     - lens and raindrops
 *  **6. Ordering of playback
 *  7. Audio support and visualization
 *  8. More transitions
 *  **9. Variable timing on items for demo
 * **10. Repeated items in a demo ordering
 * **11. Play through a demo
 * 
 * */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hypnocube.Demo.Model;
using Hypnocube.Demo.Model.Demos;
using Hypnocube.Demo.Model.Remappings;
using Hypnocube.Device;
using Hypnocube.MVVM;

namespace Hypnocube.Demo.ViewModel
{
    public sealed class DemoControlViewModel : ViewModelBase
    {
        private readonly Fps fps = new Fps();
        
        #region Device Property
        private HypnoLsdController device = null;
        /// <summary>
        /// Gets or sets the current device.
        /// </summary>
        public HypnoLsdController Device
        {
            get { return device; }
            set
            {
                // return true if there was a change.
                SetField(ref device, value);
            }
        }
        #endregion

        // used as a counter to generate sequential PNG images
        private int imageCount;
        private int tickCount;

        public DemoControlViewModel()
        {
            DemoManager = new DemoManager();
            StartStopCommand = new RelayCommand(o => StartStop());
            PlaybackImagesCommand = new RelayCommand(o => PlaybackImages());
            DeleteSettingCommand = new RelayCommand(o => DeleteSetting());
            NewSettingCommand = new RelayCommand(o => NewSetting());
            AddClipCommand = new RelayCommand(o => AddClip());
            DeleteClipCommand = new RelayCommand(o => DeleteClip());
            AddAllVisualizationsCommand = new RelayCommand(o => AddAllVisualizations());
            DeleteAllDemosCommand = new RelayCommand(o => DeleteAllDemos());

            AddSelectedVisualizationToScheduleCommand = new RelayCommand(o => AddVisualizationToSchedule(true));
            AddAllVisualizationsToScheduleCommand = new RelayCommand(o => AddVisualizationToSchedule(false));
            DeleteSelectedDemoCommand = new RelayCommand(o => DeleteSelectedDemo());
            MoveSelectedDemoUpCommand = new RelayCommand(o => MoveSelectedDemoUp());
            MoveSelectedDemoDownCommand = new RelayCommand(o => MoveSelectedDemoDown());

            ClipList = new ObservableCollection<Clipping>();
            Remappings = new ObservableCollection<Remapper>();
            Settings = new ObservableCollection<Setting>();
            Playback = new Playback(this, DemoManager,
                frame => Dispatch(() => DrawFrameToScreen(frame))
                );

            LoadItems();

            var designTime = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
                LoadSettings();
        }


        public ICommand AddAllVisualizationsCommand { get; private set; }
        public ICommand DeleteAllDemosCommand { get; private set; }

        public ICommand AddSelectedVisualizationToScheduleCommand { get; private set; }
        public ICommand AddAllVisualizationsToScheduleCommand { get; private set; }
        public ICommand DeleteSelectedDemoCommand { get; private set; }
        public ICommand MoveSelectedDemoUpCommand { get; private set; }
        public ICommand MoveSelectedDemoDownCommand { get; private set; }
        public ICommand PlaybackImagesCommand { get; private set; }

        public Playback Playback { get; private set; }

        public ObservableCollection<Remapper> Remappings { get; private set; }

        public ICommand StartStopCommand { get; private set; }

        public DemoManager DemoManager { get; private set; }
        public IMessage Messager { get; set; }

        #region SelectedVisualization Property

        private Type selectedVisualization;

        /// <summary>
        ///     Gets or sets the selected visualization.
        /// </summary>
        public Type SelectedVisualization
        {
            get { return selectedVisualization; }
            set
            {
                // return true if there was a change.
                SetField(ref selectedVisualization, value);
            }
        }

        #endregion

        #region SelectedRemapping Property

        private Remapper selectedRemapping;

        /// <summary>
        /// Gets or sets the selected image remapping.
        /// </summary>
        public Remapper SelectedRemapping
        {
            get { return selectedRemapping; }
            set
            {
                // return true if there was a change.
                if (SetField(ref selectedRemapping, value))
                    SetRemappingParameters(true);
            }
        }

        #endregion

        #region Width Property

        private int width = 100;

        /// <summary>
        ///     Gets or sets the image size.
        /// </summary>
        public int Width
        {
            get { return width; }
            set
            {
                // return true if there was a change.
                if (SetField(ref width, value))
                    Stop();
            }
        }

        #endregion

        #region Height Property

        private int height = 100;

        /// <summary>
        ///     Gets or sets the image height.
        /// </summary>
        public int Height
        {
            get { return height; }
            set
            {
                // return true if there was a change.
                if (SetField(ref height, value))
                    Stop();
            }
        }

        #endregion

        #region SettingName Property

        private string settingName = "<enter setting name here>";

        /// <summary>
        ///     Gets or sets the setting name.
        /// </summary>
        public string SettingName
        {
            get { return settingName; }
            set
            {
                // return true if there was a change.
                SetField(ref settingName, value);
            }
        }

        #endregion

        #region Strands Property

        private int strands = 1;

        /// <summary>
        ///     Gets or sets the number of strands.
        /// </summary>
        public int Strands
        {
            get { return strands; }
            set
            {
                // return true if there was a change.
                if (SetField(ref strands, value))
                    Stop();
            }
        }

        #endregion

        #region Clipping

        #region ClipX Property

        private int clipX;

        /// <summary>
        ///     Gets or sets the x clipping box.
        /// </summary>
        public int ClipX
        {
            get { return clipX; }
            set
            {
                // return true if there was a change.
                SetField(ref clipX, value);
            }
        }

        #endregion

        #region ClipY Property

        private int clipY;

        /// <summary>
        ///     Gets or sets the y clipping box.
        /// </summary>
        public int ClipY
        {
            get { return clipY; }
            set
            {
                // return true if there was a change.
                SetField(ref clipY, value);
            }
        }

        #endregion

        #region ClipWidth Property

        private int clipWidth = 16;

        /// <summary>
        ///     Gets or sets the clip width.
        /// </summary>
        public int ClipWidth
        {
            get { return clipWidth; }
            set
            {
                // return true if there was a change.
                SetField(ref clipWidth, value);
            }
        }

        #endregion

        #region ClipHeight Property

        private int clipHeight = 16;

        /// <summary>
        ///     Gets or sets the height of the clip box.
        /// </summary>
        public int ClipHeight
        {
            get { return clipHeight; }
            set
            {
                // return true if there was a change.
                SetField(ref clipHeight, value);
            }
        }

        #endregion

        public ObservableCollection<Clipping> ClipList { get; private set; }

        public ICommand AddClipCommand { get; private set; }
        public ICommand DeleteClipCommand { get; private set; }

        #region SelectedClipping Property

        private Clipping selectedClipping;

        /// <summary>
        ///     Gets or sets the selected clipping.
        /// </summary>
        public Clipping SelectedClipping
        {
            get { return selectedClipping; }
            set
            {
                // return true if there was a change.
                SetField(ref selectedClipping, value);
            }
        }

        #endregion

        private void AddClip()
        {
            ClipList.Add(new Clipping(ClipX, ClipY, ClipWidth, ClipHeight));
        }

        private void DeleteClip()
        {
            var s = SelectedClipping;
            if (s != null)
                ClipList.Remove(s);
        }


        /// <summary>
        ///     Filter flat image
        ///     Modify in place
        /// </summary>
        /// <param name="image"></param>
        private void ClipFilter(uint[] image)
        {
            var w = DemoManager.Width;
            var h = DemoManager.Height;
            if (image.Length != w*h)
                return; // cannot clip it

            // now black out regions
            foreach (var c in ClipList)
            {
                for (var i = c.X; i < c.X + c.Width; ++i)
                    for (var j = c.Y; j < c.X + c.Height; ++j)
                    {
                        var index = (i + j*w);
                        if (0 <= index && index < image.Length)
                            image[index] = 0xFF000000; // BGRA (low to high!)
                    }
            }
        }

        public class Clipping
        {
            public Clipping(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        #endregion

        #region IsRunning Property
        private bool isRunning = false;
        /// <summary>
        /// Gets or sets if the demo is running.
        /// </summary>
        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                // return true if there was a change.
                SetField(ref isRunning, value);
            }
        }
        #endregion


        #region Settings

        private string SettingFilename1 = "HCDemoSettings.txt";
        public ICommand DeleteSettingCommand { get; private set; }
        public ICommand NewSettingCommand { get; private set; }

        public ObservableCollection<Setting> Settings { get; private set; }

        #region SelectedSetting Property

        private Setting selectedSetting;

        /// <summary>
        ///     Gets or sets the selected setting item.
        /// </summary>
        public Setting SelectedSetting
        {
            get { return selectedSetting; }
            set
            {
                // return true if there was a change.
                var lastSetting = selectedSetting;
                if (SetField(ref selectedSetting, value))
                    ChangeSettings(lastSetting);
            }
        }

        #endregion

        private void AddAllVisualizations()
        {
            foreach (var vis in DemoManager.VisualizationTypes)
                DemoManager.VisualizationSchedule.Add(new DemoManager.VisualizationWrapper(vis, AnimationLength));
        }

        private void DeleteAllDemos()
        {
            DemoManager.VisualizationSchedule.Clear();
            SelectedDemoIndex = -1;
        }

        /// <summary>
        ///     Save the given setting. If null, do nothing
        /// </summary>
        /// <param name="setting"></param>
        private void SaveSettings(Setting setting)
        {
            if (setting == null)
                return;
            setting.SaveViewModel(this);
        }

        /// <summary>
        /// Used for save/load location
        /// </summary>
        private static string filePath;

        static DemoControlViewModel()
        {
            filePath = FileTools.GetUserStoragePath("Hypnocube","HypnoDemo"); 
        }

        // load all settings from  file
        private void LoadSettings()
        {
            var filename = Path.Combine(filePath, SettingFilename1);
            if (!File.Exists(filename))
            {
                MessageBox.Show("Cannot find settings file", "HypnoDemo");
                return;
            }
            using (var file = File.OpenText(filename))
            {
                var countText = file.ReadLine();
                int count;
                if (!Int32.TryParse(countText, out count))
                {
                    MessageBox.Show("Corrupt settings file");
                    return;
                }
                Settings.Clear();
                for (var i = 0; i < count; ++i)
                {
                    var s = Setting.Read(file, DemoManager);
                    if (s == null)
                    {
                        MessageBox.Show(String.Format("Error in settings file after {0} settings read", i));
                        return;
                    }
                    Settings.Add(s);
                }
            }
        }


        /// <summary>
        ///     Save all settings to file
        /// </summary>
        public void SaveSettings()
        {
            if (SelectedSetting != null)
                SelectedSetting.SaveViewModel(this);
            var filename = Path.Combine(filePath, SettingFilename1);
            if (!string.IsNullOrEmpty(filename))
            {
                using (var file = File.CreateText(filename))
                {
                    file.WriteLine(Settings.Count);
                    foreach (var setting in Settings)
                        setting.Write(file);
                }
            }
        }

        private void NewSetting()
        {
            var s = new Setting(SettingName);
            s.SaveViewModel(this);
            Settings.Add(s);
            SelectedSetting = s;
        }

        private void DeleteSetting()
        {
            var s = SelectedSetting;
            if (s == null)
            {
                MessageBox.Show("Select a setting first");
                return;
            }
            var index = Settings.IndexOf(s);
            if (index == Settings.Count - 1)
                index--;
            Settings.Remove(s);
            if (0 <= index && index < Settings.Count)
                SelectedSetting = Settings[index];
            else
                SelectedSetting = null;
        }

        public class Setting
        {
            public Setting(string name)
            {
                Name = name;
                DemoSchedule = new List<DemoManager.VisualizationWrapper>();
            }


            public string Name { get; set; }
            public string PortName { get; set; }
            public int BaudRate { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public bool CycleAnimation { get; set; }
            public int TimePerAnimation { get; set; }
            public string RemappingName { get; set; }
            public double Brightness { get; set; }
            public int FramesPerSecond { get; set; }
            public List<DemoManager.VisualizationWrapper> DemoSchedule { get; private set; }
            public bool UseGamma { get; set; }
            public double Gamma { get; set; }
            public int Strands { get; set; }

            /// <summary>
            ///     save model to setting
            /// </summary>
            /// <param name="model"></param>
            public void SaveViewModel(DemoControlViewModel model)
            {
                Width = model.Width;
                Height = model.Height;
                Brightness = model.Brightness;
                CycleAnimation = model.CycleAnimations;
                FramesPerSecond = model.FramesPerSecond;
                PortName = model.Device == null ? "" : model.Device.PortName;
                TimePerAnimation = model.AnimationLength;
                Strands = model.Strands;
                Gamma = model.GammaCorrection;
                UseGamma = model.UseGammaCorrection;
                BaudRate = model.BaudRate;

                DemoSchedule.Clear();
                foreach (var v in model.DemoManager.VisualizationSchedule)
                    DemoSchedule.Add(v);
                if (model.SelectedRemapping != null)
                    RemappingName = model.SelectedRemapping.GetType().Name;
                else
                    RemappingName = "";
            }

            /// <summary>
            ///     load model from setting
            /// </summary>
            /// <param name="model"></param>
            public void LoadViewModel(DemoControlViewModel model)
            {
                model.Width = Width;
                model.Height = Height;
                model.Brightness = Brightness;
                model.CycleAnimations = CycleAnimation;
                model.FramesPerSecond = FramesPerSecond;
                if (model.Device != null)
                    model.Device.PortName = PortName;
                model.AnimationLength = TimePerAnimation;
                model.Strands = Strands;
                model.GammaCorrection = Gamma;
                model.UseGammaCorrection = UseGamma;
                model.BaudRate = BaudRate;

                model.DemoManager.VisualizationSchedule.Clear();
                foreach (var v in DemoSchedule)
                    model.DemoManager.VisualizationSchedule.Add(v);

                model.SelectedRemapping = model.Remappings.FirstOrDefault(z => z.GetType().Name == RemappingName);
            }

            public override string ToString()
            {
                return Name;
            }


            /// <summary>
            ///     Read a setting from a text representation
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            internal static Setting Read(StreamReader input, DemoManager demoManager)
            {
                var visualizations = demoManager.VisualizationTypes;
                if (input == null)
                    return null;
                try
                {
                    var name = input.ReadLine();
                    var s = new Setting(name);
                    s.Brightness = Double.Parse(input.ReadLine());
                    s.CycleAnimation = input.ReadLine().ToLower() == "true";
                    s.FramesPerSecond = Int32.Parse(input.ReadLine());
                    s.Width = Int32.Parse(input.ReadLine());
                    s.Height = Int32.Parse(input.ReadLine());
                    s.PortName = input.ReadLine();
                    s.RemappingName = input.ReadLine();
                    s.TimePerAnimation = Int32.Parse(input.ReadLine());
                    s.Strands = Int32.Parse(input.ReadLine());
                    s.Gamma = Double.Parse(input.ReadLine());
                    s.UseGamma = input.ReadLine().ToLower() == "true";
                    s.BaudRate = Int32.Parse(input.ReadLine());
                    var visCount = Int32.Parse(input.ReadLine());

                    for (var i = 0; i < visCount; ++i)
                    {
                        var line = input.ReadLine();
                        var parameterText = "";
                        var pti = line.IndexOf(':');
                        if (pti != -1)
                        {
                            parameterText = line.Substring(pti + 1);
                            line = line.Substring(0, pti - 1);
                        }
                        
                        var words = line.Split(' ');
                        int length;
                        if (words.Length == 2 && Int32.TryParse(words[1], out length))
                        {
                            var dname = words[0];
                            var type = visualizations.First(t => t.Name == dname);
                            var v = new DemoManager.VisualizationWrapper(type, length);
                            v.ParameterText = parameterText;
                            s.DemoSchedule.Add(v);
                        }
                    }
                    return s;
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception loading setting");
                    Trace.TraceError(e.ToString());
                }
                return null;
            }

            /// <summary>
            ///     Write this setting to a text representation
            /// </summary>
            /// <param name="output"></param>
            internal void Write(StreamWriter output)
            {
                output.WriteLine(Name);
                output.WriteLine(Brightness);
                output.WriteLine(CycleAnimation);
                output.WriteLine(FramesPerSecond);
                output.WriteLine(Width);
                output.WriteLine(Height);
                output.WriteLine(PortName);
                output.WriteLine(RemappingName);
                output.WriteLine(TimePerAnimation);
                output.WriteLine(Strands);
                output.WriteLine(Gamma);
                output.WriteLine(UseGamma);
                output.WriteLine(BaudRate);
                output.WriteLine(DemoSchedule.Count);
                foreach (var v in DemoSchedule)
                {
                    var pt = v.ParameterText;
                    if (pt == null)
                        pt = "";
                    output.WriteLine(v.DemoType.Name + " " + v.FrameLength + " : " + pt);
                }
            }
        }

        #region BaudRate Property

        private int baudRate = 3000000;

        /// <summary>
        ///     Gets or sets the desired baudrate.
        /// </summary>
        public int BaudRate
        {
            get { return baudRate; }
            set
            {
                // return true if there was a change.
                SetField(ref baudRate, value);
            }
        }

        #endregion

        #endregion

        private void PlaybackImages()
        {
            MessageBox.Show("TODO: Not implemented");
        }

        private void MoveSelectedDemoDown()
        {
            var i = SelectedDemoIndex;
            if (i < 0)
            {
                MessageBox.Show("Select demo first");
                return;
            }
            if (i < DemoManager.VisualizationSchedule.Count - 1)
            {
                var d = DemoManager.VisualizationSchedule[i];
                var t = SelectedDemoIndex;
                DemoManager.VisualizationSchedule.RemoveAt(i);
                DemoManager.VisualizationSchedule.Insert(i + 1, d);
                SelectedDemoIndex = t + 1;
            }
        }

        private void MoveSelectedDemoUp()
        {
            var i = SelectedDemoIndex;
            if (i < 0)
            {
                MessageBox.Show("Select demo first");
                return;
            }
            if (i > 0)
            {
                var d = DemoManager.VisualizationSchedule[i];
                var t = SelectedDemoIndex;
                DemoManager.VisualizationSchedule.RemoveAt(i);
                DemoManager.VisualizationSchedule.Insert(i - 1, d);
                SelectedDemoIndex = t - 1;
            }
        }

        private void DeleteSelectedDemo()
        {
            var i = SelectedDemoIndex;
            if (i < 0)
            {
                MessageBox.Show("Select demo first");
                return;
            }
            DemoManager.VisualizationSchedule.RemoveAt(i);
            if (i >= DemoManager.VisualizationSchedule.Count)
                SelectedDemoIndex--;
        }

        private void AddVisualizationToSchedule(bool addSelectedOnly)
        {
            if (addSelectedOnly)
            {
                var s = SelectedVisualization;
                if (s == null)
                {
                    MessageBox.Show("Select a visualization first");
                    return;
                }

                DemoManager.VisualizationSchedule.Add(new DemoManager.VisualizationWrapper(s, AnimationLength));
            }
            else
            {
                foreach (var s in DemoManager.VisualizationTypes)
                DemoManager.VisualizationSchedule.Add(new DemoManager.VisualizationWrapper(s, AnimationLength));
            }
            SelectedDemoIndex = DemoManager.VisualizationSchedule.Count - 1;
        }

        private void LoadItems()
        {
            foreach (var remapper in DemoManager.Remappers)
                Remappings.Add(remapper);
        }

        /// <summary>
        /// Update settings based on selected remapping, width, height, etc.
        /// </summary>
        private void SetRemappingParameters(bool remappingChanged)
        {
            var s = SelectedRemapping;
            if (s != null)
            {
                if (remappingChanged)
                {
                    Strands = s.Strands;
                    Width   = s.Width;
                    Height  = s.Height;
                }
                else
                {
                    s.Strands = Strands;
                    s.Width = Width;
                    s.Height = Height;
                }
            }
        }

        private void ChangeSettings(Setting lastSetting)
        {
            SaveSettings(lastSetting);
            var s = SelectedSetting;
            if (s == null)
                return; // make no changes for these

            SettingName = s.Name;

            s.LoadViewModel(this);
            Start();
        }

        /// <summary>
        /// Stop any running demo
        /// </summary>
        public void Stop()
        {
            Playback.Stop();
            IsRunning = false;
        }

        void StartStop()
        {
            if (IsRunning)
                Stop();
            else
                Start();
        }
        public void Start()
        {
            if (SelectedRemapping == null)
            {
                MessageBox.Show("Select a remapping first");
                return;
            }
            if (DemoManager.VisualizationSchedule.Count <= 0)
            {
                MessageBox.Show("No demos added to schedule");
                return;
            }
            if (Device == null || Device.IsConnected == false)
            {
                MessageBox.Show("Must be connected to a HypnoLSD module first");
                return;
            }

            SetRemappingParameters(false);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (SelectedDemoIndex < 0)
                        SelectedDemoIndex = 0;
                    DemoManager.Start(Device, SelectedRemapping, BaudRate, ClipFilter);
                }
                catch (Exception e)
                {
                    Messager.AddError(e.ToString());
                }
                Playback.Start();

                IsRunning = true;

                Thread.Sleep(1000);

                if (EnableLoggingAction != null)
                    EnableLoggingAction(false); // otherwise kills gui


            });


        }

        public Action<bool> EnableLoggingAction { get; set; }

        /// <summary>
        ///     Called once per frame, generates images
        /// </summary>
        private void DrawFrameToScreen(uint[] frame)
        {
            if (CycleAnimations)
            {
                tickCount++;
                var timeMs = AnimationLength; // ms per animation
                if ((tickCount%(timeMs/FramesPerSecond)) == 0)
                    Next();
            }

            // ensure the screen bitmap is the right size and format
            var h = DemoManager.Height;
            var w = DemoManager.Width;
            var img = ImageSource;
            if (img == null)
                img = new WriteableBitmap(w, h, 96.0, 96.0, PixelFormats.Bgra32, null);

            var bmp = img as WriteableBitmap;
            if (bmp == null) return; // something wrong, return

            if (bmp.PixelWidth != w || bmp.PixelHeight != h)
                bmp = new WriteableBitmap(w, h, 96.0, 96.0, PixelFormats.Bgra32, null);

            bmp.WritePixels(new Int32Rect(0, 0, w, h), frame, w*4, 0);

            ActualFramesPerSecond = fps.Tick();
            ImageSource = null; // toggle update
            ImageSource = bmp;

            if (SaveImages)
                SaveImage(bmp, ImagePlayback.Filename(filePath,imageCount++));
        }

        private void SaveImage(WriteableBitmap bmp, string filename)
        {
            var encoder = new PngBitmapEncoder();
            var frame = BitmapFrame.Create(bmp);
            encoder.Frames.Add(frame);
            using (var stream = File.Create(filename))
            {
                encoder.Save(stream);
            }
        }

        /// <summary>
        ///     next animation
        /// </summary>
        internal void Next()
        {
#if false
    // get next allowed vis
            var current = SelectedDemoIndex;
            var next = current;
            do
            {
                next = (next + 1) % DemoManager.VisualizationSchedule.Count;
            } while (!DemoManager.VisualizationSchedule[next].Scheduled && next != current);
            SelectedDemoIndex = next;
#endif
        }

        #region AnimationLength Property

        private int animationLength = 900;

        /// <summary>
        ///     Gets or sets animation length in ms.
        /// </summary>
        public int AnimationLength
        {
            get { return animationLength; }
            set
            {
                // return true if there was a change.
                SetField(ref animationLength, value);
            }
        }

        #endregion

        #region ImageSource Property

        private ImageSource imageSource;

        /// <summary>
        ///     Gets or sets the current image source.
        /// </summary>
        public ImageSource ImageSource
        {
            get { return imageSource; }
            set
            {
                // return true if there was a change.
                SetField(ref imageSource, value);
            }
        }

        #endregion

        #region Brightness Property

        private double brightness = 100;

        /// <summary>
        ///     Gets or sets the image brightness 0-100.
        /// </summary>
        public double Brightness
        {
            get { return brightness; }
            set
            {
                // return true if there was a change.
                if (SetField(ref brightness, value))
                    DemoManager.Brightness = brightness;
            }
        }

        #endregion

        #region SaveImages Property

        private bool saveImages;

        /// <summary>
        ///     Gets or sets if images are saved during playback.
        /// </summary>
        public bool SaveImages
        {
            get { return saveImages; }
            set
            {
                // return true if there was a change.
                if (SetField(ref saveImages, value))
                    imageCount = 0;
            }
        }

        #endregion

        #region CycleAnimations Property

        private bool cycleAnimations = true;

        /// <summary>
        ///     Gets or sets whether or not to cycle animations.
        /// </summary>
        public bool CycleAnimations
        {
            get { return cycleAnimations; }
            set
            {
                // return true if there was a change.
                SetField(ref cycleAnimations, value);
            }
        }

        #endregion

        #region SelectedScheduledDemo Property
        private DemoManager.VisualizationWrapper selectedScheduledDemo = null;
        /// <summary>
        /// Gets or sets the current demo.
        /// </summary>
        public DemoManager.VisualizationWrapper SelectedScheduledDemo
        {
            get { return selectedScheduledDemo; }
            set
            {
                // return true if there was a change.
                SetField(ref selectedScheduledDemo, value);
            }
        }
        #endregion


        #region SelectedDemoIndex Property

        private int selectedDemoIndex;

        /// <summary>
        ///     Gets or sets the selected demo.
        /// </summary>
        public int SelectedDemoIndex
        {
            get { return selectedDemoIndex; }
            set
            {
                // return true if there was a change.
                SetField(ref selectedDemoIndex, value);
            }
        }

        #endregion

        #region ActualFramesPerSecond Property

        private double actualFramesPerSecond = 30;

        /// <summary>
        ///     Gets or sets the actual frames per second.
        /// </summary>
        public double ActualFramesPerSecond
        {
            get { return actualFramesPerSecond; }
            set
            {
                // return true if there was a change.
                SetField(ref actualFramesPerSecond, value);
            }
        }

        #endregion

        #region GammaCorrection Property

        private double gammaCorrection = 2.22;

        /// <summary>
        ///     Gets or sets the gamma correction.
        /// </summary>
        public double GammaCorrection
        {
            get { return gammaCorrection; }
            set
            {
                // return true if there was a change.
                SetField(ref gammaCorrection, value);
            }
        }

        #endregion

        #region FramesPerSecond Property

        private int framesPerSecond = 30;

        /// <summary>
        ///     Gets or sets the attempted frames per second.
        /// </summary>
        public int FramesPerSecond
        {
            get { return framesPerSecond; }
            set
            {
                // return true if there was a change.
                SetField(ref framesPerSecond, value);
            }
        }

        #endregion

        #region UseGammaCorrection Property

        private bool useGammaCorrection = true;

        /// <summary>
        ///     Gets or sets to use gamma correction.
        /// </summary>
        public bool UseGammaCorrection
        {
            get { return useGammaCorrection; }
            set
            {
                // return true if there was a change.
                SetField(ref useGammaCorrection, value);
            }
        }

        #endregion

        #region DemoParameterText Property
        private string demoParameterText = "";
        /// <summary>
        /// Gets or sets the parameter text for the selected demo.
        /// </summary>
        public string DemoParameterText
        {
            get { return demoParameterText; }
            set
            {
                // return true if there was a change.
                SetField(ref demoParameterText, value);
            }
        }
        #endregion


    }
}