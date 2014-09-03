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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Hypnocube.Communications;
using Hypnocube.Device;
using Hypnocube.MVVM;
using Hypnocube.SerialTester.Model;
using Microsoft.Win32;

namespace Hypnocube.SerialTester.ViewModel
{
    public class SerialTesterViewModel : ViewModelBase
    {
        /// <summary>
        ///     Object used for multiple thread access to device
        /// </summary>
        private readonly object picLock = new object();

        private DispatcherTimer frameTimer;
        private int testCount = 0;
        private TestImage testImage;
        private int testImageFrame;

        public SerialTesterViewModel()
        {
            Rates = new ObservableCollection<BaudRateSettings>();
            Members = new ObservableCollection<MemberInfo>();
            SerialSpeeds = new ObservableCollection<BaudRateSettings>();

            SwitchSpeedCommand = new RelayCommand(o => SwitchSpeed());
            SetSizeCommand = new RelayCommand(o => SetSize());
            SendBytesCommand = new RelayCommand(o => SendBytes());
            DumpInfoCommand = new RelayCommand(o => DumpInfo());
            DumpStatsCommand = new RelayCommand(o => DumpStats());
            VersionCommand = new RelayCommand(o => GetVersion());
            SendRedCommand = new RelayCommand(o => SendBytes(0, 255, 0));
            SendGreenCommand = new RelayCommand(o => SendBytes(255, 0, 0));
            SendBlueCommand = new RelayCommand(o => SendBytes(0, 0, 255));
            SendSyncCommand = new RelayCommand(o => SendBytes(new[] {HypnoLsdController.SyncByte}));
            MidCommand = new RelayCommand(o => SendBytes(99, 99, 99));
            DumpRAMCommand = new RelayCommand(o => DumpRAM());
            ToggleDrawModeCommand = new RelayCommand(o => ToggleDrawMode());
            TestImageCommand = new RelayCommand(o => TestImage());
            TestCpCommand = new RelayCommand(o => TestCp());
            ThroughputTestCommand = new RelayCommand(o => TestThroughput());
            ResetStatsCommand = new RelayCommand(o => ResetStats());
            TestSerialBufferCommand = new RelayCommand(o => TestSerialBuffer());
            DrawRandomImageCommand = new RelayCommand(o => DrawRandomImage());
            TestFinalImageCommand = new RelayCommand(o => TestFinalImage());

            HelpCommand = new RelayCommand(o => Device.GetHelp());
            TestSkewCommand = new RelayCommand(o => TestSkew());
            SizesCommand = new RelayCommand(o => Device.GetSizes());
            SaveSettingsCommand = new RelayCommand(o => Device.SaveSettings());
            GetSpeedCommand = new RelayCommand(o => Device.GetSpeed());
            GetSizeCommand = new RelayCommand(o => Device.GetSize());
            GetIdCommand = new RelayCommand(o => Device.GetId());
            SetIdCommand = new RelayCommand(o => SetId());
            DumpTimingsCommand = new RelayCommand(o => Device.DumpTimings());
            SetDemoDelayCommand = new RelayCommand(o => SetDemoDelay());
            RunDemoCommand = new RelayCommand(o => RunDemo());
            GetFileCommand = new RelayCommand(o => GetFile());
            PutFileCommand = new RelayCommand(o => PutFile());

        }

        #region Device Property
        private HypnoLsdController device = null;
        /// <summary>
        /// Gets or sets the connected device.
        /// </summary>
        public HypnoLsdController Device
        {
            get { return device; }
            set
            {
                var oldDevice = device;
                // return true if there was a change.
                if (SetField(ref device, value))
                    DeviceChanged(oldDevice,device);
            }
        }

        #endregion


        /// <summary>
        /// Device changed, sets internals accordingly,
        /// </summary>
        private void DeviceChanged(HypnoLsdController oldDevice, HypnoLsdController newDevice)
        {
            if (oldDevice != null)
                oldDevice.PropertyChanged -= DeviceOnPropertyChanged;
            if (newDevice != null)
            {
                IsConnectedAndNotDrawing = Device.IsConnected && !device.Drawing;
                device.PropertyChanged += DeviceOnPropertyChanged;

                RamSize = newDevice.RamSize;
                
                PrepareSerialSpeeds(newDevice.PicClockSpeed);

                // todo - make baud rate of device chosen
                SelectedSpeed = SerialSpeeds[0];

                ProcessReflection(newDevice.GetType());

                ComputeRateParameters(newDevice.PicClockSpeed);


            }

        }

        private void DeviceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            IsConnectedAndNotDrawing = Device.IsConnected && !device.Drawing;
        }

        /// <summary>
        /// baud rates and errors
        /// Ordered by PIC32, FTDI low, FTDI High
        /// </summary>
        public ObservableCollection<BaudRateSettings> Rates { get; private set; }

        /// <summary>
        /// Available Serial clock speeds
        /// </summary>
        public ObservableCollection<BaudRateSettings> SerialSpeeds { get; private set; }

        /// <summary>
        ///     How much ram the devise has
        /// </summary>
        public int RamSize { get; private set; }


        /// <summary>
        ///     Members exported from the controller class
        /// </summary>
        public ObservableCollection<MemberInfo> Members { get; private set; }

        public ICommand SetDemoDelayCommand { get; private set; }

        public ICommand RunDemoCommand { get; private set; }
        public ICommand GetFileCommand { get; private set; }
        public ICommand PutFileCommand { get; private set; }

        public ICommand TestSkewCommand { get; private set; }
        public ICommand HelpCommand { get; private set; }
        public ICommand SizesCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand GetSpeedCommand { get; private set; }
        public ICommand GetSizeCommand { get; private set; }
        public ICommand GetIdCommand { get; private set; }
        public ICommand SetIdCommand { get; private set; }
        public ICommand DumpTimingsCommand { get; private set; }
        public IMessage Messager { get; set; }
        public ICommand DrawRandomImageCommand { get; private set; }
        public ICommand ThroughputTestCommand { get; private set; }
        public ICommand ResetStatsCommand { get; private set; }
        public ICommand SendBytesCommand { get; private set; }

        /// <summary>
        ///     Switch to the selected speed.
        /// </summary>
        public ICommand SwitchSpeedCommand { get; private set; }

        public ICommand SetSizeCommand { get; private set; }
        public ICommand TestSerialBufferCommand { get; private set; }
        public ICommand VersionCommand { get; private set; }
        public ICommand DumpInfoCommand { get; private set; }
        public ICommand DumpStatsCommand { get; private set; }
        public ICommand DumpRAMCommand { get; private set; }
        public ICommand SendRedCommand { get; private set; }
        public ICommand SendGreenCommand { get; private set; }
        public ICommand SendBlueCommand { get; private set; }
        public ICommand SendSyncCommand { get; private set; }
        public ICommand MidCommand { get; private set; }

        public ICommand TestCpCommand { get; private set; }

        public ICommand TestFinalImageCommand { get; private set; }
        public ICommand ToggleDrawModeCommand { get; private set; }
        public ICommand TestImageCommand { get; private set; }

        /// <summary>
        ///     Show a message on the screen
        /// </summary>
        private void ShowMessageBox(string message)
        {
            // todo - inject from View, or process there...
            MessageBox.Show(message);
        }

        /// <summary>
        ///     Connect, using optional drive name if present
        /// </summary>
        /// <param name="driveName"></param>
        private void Connect(string driveName = null)
        {
            lock (picLock)
            {
                if ((Device != null) && (Device.IsConnected))
                {
                    Device.Close();
                    Device = new HypnoLsdController();
                    return;
                }
                else
                {
                    // try to connect
                    Device.MessageReceived += BasicListener;
                    Device.MessageReceived += BinaryListener;
#if false
                    Messages.Add("Faking connection - TODO - fix");
                    IsConnectedAndNotDrawing = !IsConnectedAndNotDrawing;
                    IsConnected = !IsConnected;
#else
                    if (false == Device.Open(driveName))
                    {
                        ShowMessageBox("Did not connect");
                    }
                    else
                    {
                        ShowMessageBox("Needs fixing - check code");
                        //IsConnected = Device.IsConnected;
                        //IsConnectedAndNotDrawing = isConnected;
                    }
#endif
                }
            }
        }

        private void PrepareSerialSpeeds(int picClockSpeed)
        {
            int[] desiredRates =
            {
                9600,
                19200,
                38400,
                56000,
                57600,
                115200,
                250000,
                500000,
                750000,
                1000000
            };

            // create possible PIC32 UART speeds, given by
            // picClockRate/(4*(num+1)) for num = 1 to 65535
            SerialSpeeds.Clear();
            foreach (var targetRate in desiredRates)
                SerialSpeeds.Add(BaudRateCalculator.Compute(SerialDeviceType.PIC32, targetRate, picClockSpeed));

            // add the rest of the speeds that hit exactly
            var rate = desiredRates[desiredRates.Length - 1];

            var br = BaudRateCalculator.Compute(SerialDeviceType.PIC32, rate, picClockSpeed);
            var divider = Int32.Parse(br.Setting);
            while (divider > 0)
            {
                --divider;
                if ((picClockSpeed%(divider + 1)) == 0)
                {
                    rate = BaudRateCalculator.ComputePic32BaudRateFromClockDivider(picClockSpeed,divider);
                    SerialSpeeds.Add(BaudRateCalculator.Compute(SerialDeviceType.PIC32, rate, picClockSpeed));
                }
            }
        }

        private void ProcessReflection(Type type)
        {
//            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
//                    where t.IsClass && t.CustomAttributes.Contains(null)
//                    select t;
            foreach (var tt in type.GetMembers())
            {
                var att = tt.GetCustomAttributes<FeatureAttribute>();
                if (att == null || !att.Any())
                    continue;
                Members.Add(tt);
            }
        }

        private void TimerTick()
        {
            var b = 80; // brightness
            var d = 2; // darkness
            var len = 3;
            var spacing = 22;
            var delta = 0; // block strand delta
            testImage.GenerateTestImage(testImageFrame, 3, b, d, len, spacing, delta, true, true);
            Device.WriteImage(testImage.data);
            ++testImageFrame;
        }

        private void TestFinalImage()
        {
            if (frameTimer == null)
            {
                frameTimer = new DispatcherTimer();
                frameTimer.Tick += (o, e) => TimerTick();
                frameTimer.Interval = TimeSpan.FromMilliseconds(1000/40);
            }

            if (!frameTimer.IsEnabled)
            {
                var width = Device.ImageWidth;
                var height = Device.ImageHeight;
                testImage = new TestImage(width, height, Device.ImageSize);
                testImageFrame = 0;
            }

            frameTimer.IsEnabled = !frameTimer.IsEnabled;
        }

        public void RunDemo()
        {
            int demoIndex, demoLength;
            if (Int32.TryParse(DemoIndexText, out demoIndex) && Int32.TryParse(DemoLengthText, out demoLength)
                && demoIndex >= 0 && demoLength >= 0)
                Device.RunDemo(demoIndex, demoLength);
            else
                MessageBox.Show("demo length or index invalid");
        }

        public void GetFile()
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == true)
            {
                var ft = new FileTransfer();
                ft.GetFile(Device, sfd.FileName);
            }
        }

        public void PutFile()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var ft = new FileTransfer();
                ft.PutFile(Device, ofd.FileName);
            }
        }

        public void SetDemoDelay()
        {
            int delay;
            if (Int32.TryParse(DemoDelayText, out delay) && delay >= 0 && delay < 65536)
                Device.SetDemoDelay(delay);
            else
                MessageBox.Show("Delay amount not a valid integer");
        }


        public void SetId()
        {
            int id;
            if (Int32.TryParse(IdBufferText, out id))
                Device.SetId((ushort) id);
            else
                MessageBox.Show("Illegal id value. Must be in 0-65535.");
        }

        private void TestSkew()
        {
            var bytes = GetSendBytes();
            if (bytes.Length < 3)
            {
                MessageBox.Show("Need at least 3 bytes to send");
            }
            else
                Device.SkewTest(SkewDelay1, SkewDelay2, SkewDelay3, Device.ImageWidth*Device.ImageHeight, bytes[0],
                    bytes[1], bytes[2]);
        }

        public void ResetStats()
        {
            Device.ResetStats();
        }


        private void BinaryListener(object sender, SerialDeviceBase.MessageReceivedArgs args)
        {
            Dispatch(() => Messager.AddMessage(args.Data, true));
        }


        private void BasicListener(object sender, SerialDeviceBase.MessageReceivedArgs args)
        {
            Dispatch(() => Messager.AddMessage(args.Data, false));
        }

        /// <summary>
        ///     Draw an image using the given pixel function
        /// </summary>
        /// <param name="getPixel"></param>
        private void DrawImage(Func<Color> getPixel)
        {
            lock (picLock)
            {
                if ((Device != null) && (Device.IsConnected))
                {
                    var image = new byte[Device.ImageSize];
                    for (var i = 0; i < Device.ImageSize; i += 3)
                    {
                        var color = getPixel();
                        image[i] = color.R;
                        image[i + 1] = color.G;
                        image[i + 2] = color.B;
                    }
                    Device.WriteImage(image);
                }
            }
        }

        private void DrawRandomImage()
        {
            var r = new Random();
            DrawImage(() => Color.FromRgb((byte) r.Next(), (byte) r.Next(), (byte) r.Next()));
        }

        /// <summary>
        ///     Lock the device, check it is valid, then perform the action
        /// </summary>
        /// <param name="deviceAction"></param>
        private void DeviceAction(Action deviceAction)
        {
            lock (picLock)
            {
                if ((Device != null) && (Device.IsConnected))
                    deviceAction();
            }
        }

        private void PixelSliderUpdate()
        {
            // create single pixel image
            var size = 0;
            var index = (int) PixelSliderValue*3;
            DeviceAction(() => size = Device.ImageSize);
            var image = new byte[size];

            byte r, g, b;
            GetColor(out r, out g, out b);

            image[index++] = r;
            image[index++] = g;
            image[index] = b;
            DeviceAction(() => Device.WriteImage(image));
        }

        private void GetColor(out byte r, out byte g, out byte b)
        {
            r = g = b = 255; // default to something to see
            r = (byte) RedColorValue;
            g = (byte) GreenColorValue;
            b = (byte) BlueColorValue;
        }

        private void ColorChanged()
        {
            byte r, g, b;
            GetColor(out r, out g, out b);
            // note set to GRB strand type
            DrawImage(() => Color.FromArgb(255, g, r, b));
        }

        private void TestThroughput()
        {
            if (!Device.IsConnected)
                return;
            var buffer = new byte[Device.ImageSize];
            var r = new Random();
            var start = Environment.TickCount;
            var frames = 0;
            var time = 2000; // ms to run
            var drawing = Device.Drawing;
            if (!drawing)
            {
                Device.Drawing = true;
                Thread.Sleep(1000);
            }
            DeviceAction(() =>
            {
                while (Environment.TickCount - start < 2000)
                {
                    r.NextBytes(buffer);
                    Device.WriteImage(buffer);
                    ++frames;
                }
                if (!drawing)
                {
                    Device.Drawing = false;
                    Thread.Sleep(1000);
                }
            });
            ThroughputFps = 1000.0*frames/time;
        }

        private byte[] GetSendBytes()
        {
            var text = SendByteText.Replace(',', ' ').Trim();
            var words = text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var temp = 0;
            var onePass = words.
                Where(w => Int32.TryParse(w, out temp)).
                Select(r => temp).
                Where(d => 0 <= d && d <= 255).
                Select(d => (byte) d).
                ToArray();
            if (onePass.Length == words.Length)
                return onePass;
            else
                ShowMessageBox("Byte value must be in 0 to 255");
            return null;
        }

        private void SendBytes()
        {
            var onePass = GetSendBytes();
            int repeatCount;
            if (!Int32.TryParse(RepeatByteText, out repeatCount))
            {
                ShowMessageBox("Repeat count must be a positive integer");
                return;
            }
            if (onePass != null)
            {
                var vals = new byte[onePass.Length*repeatCount];
                for (var i = 0; i < vals.Length; ++i)
                    vals[i] = onePass[i%onePass.Length];
                Device.WriteBytes(vals);
            }
        }

        private void SwitchSpeed()
        {
            try
            {
                int rate;
                if (Int32.TryParse(DesiredBaudRateText, out rate))
                {
                    Device.ConnectionSpeed = rate;
                }
                else MessageBox.Show("Select one speed first to try");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void SetSize()
        {
            int width, height;
            if (Int32.TryParse(WidthText, out width) && Int32.TryParse(HeightText, out height))
                Device.SetSize(width, height);
            else
                ShowMessageBox("Invalid size");
        }

        private void TestSerialBuffer()
        {
            if (1 <= TestBufferCount && TestBufferCount <= 100000 && 1 <= TestBufferSize &&
                TestBufferSize <= Device.RamSize)
            {
                var elapsed = Device.TestConnection((ushort) TestBufferSize, (uint) TestBufferCount,
                    InjectTestingErrors ? 1 : 0);
                Messager.AddMessage("Used " + elapsed + " ms.\n\r");
            }
            else
            {
                ShowMessageBox("Size and count must be valid.");
            }
        }

        private void GetVersion()
        {
            Device.GetVersion();
        }

        private void DumpInfo()
        {
            Device.DumpInfo();
        }

        private void DumpStats()
        {
            Device.DumpStats();
        }

        private void DumpRAM()
        {
            ushort size;
            if (UInt16.TryParse(DumpRamText, out size))
                Device.DumpRam(size);
            else
                ShowMessageBox("Invalid size " + size);
        }

        private void SendBytes(params byte[] bytes)
        {
            if (bytes.Length > 0)
                Device.WriteBytes(bytes);
        }

        private void TestCp()
        {
            // create test bytes, stream through
            var bytes = new List<byte>();

            ++testCount;
            var r = (byte) ((testCount & 1) == 0 ? 0 : 255);
            var g = (byte) ((testCount & 2) == 0 ? 0 : 255);
            var b = (byte) ((testCount & 4) == 0 ? 0 : 255);
            // add some color bytes
            for (var i = 0; i < 113; ++i)
            {
                bytes.Add(5);
                //bytes.Add(g);
                //bytes.Add(b);
            }

            // enter drawing
            Device.Drawing = true;
            Thread.Sleep(10); // wait a moment

            // execute it
            Device.WriteBytes(bytes.ToArray());
            Thread.Sleep(10); // wait a moment

            // exit drawing mode
            Device.Drawing = false;
            Thread.Sleep(10); // wait a moment

            // dump stats
            Device.DumpStats();
            Thread.Sleep(10); // wait a moment

        }

        private void ToggleDrawMode()
        {
            Device.Drawing = !Device.Drawing;
        }

        // test the remote image buffer
        public void TestImage()
        {
            var supp = new Support();

            // stop normal listener
            Device.MessageReceived -= BasicListener;
            Device.MessageReceived -= BinaryListener;

            supp.TestImage(Device, Messager.AddMessage);

            // restore handlers
            Device.MessageReceived += BasicListener;
            Device.MessageReceived += BinaryListener;
        }

        #region DumpRamText Property

        private string dumpRamText = "1000";

        /// <summary>
        ///     Gets or sets the amount of RAM to dump.
        /// </summary>
        public string DumpRamText
        {
            get { return dumpRamText; }
            set
            {
                // return true on change
                SetField(ref dumpRamText, value);
            }
        }

        #endregion

        #region TestBufferSize Property

        private int testBufferSize = 1000;

        /// <summary>
        ///     Gets or sets the size of a test buffer.
        /// </summary>
        public int TestBufferSize
        {
            get { return testBufferSize; }
            set
            {
                // return true on change
                SetField(ref testBufferSize, value);
            }
        }

        #endregion

        #region TestBufferCount Property

        private int testBufferCount = 10;

        /// <summary>
        ///     Gets or sets the number of times to run the buffer.
        /// </summary>
        public int TestBufferCount
        {
            get { return testBufferCount; }
            set
            {
                // return true on change
                SetField(ref testBufferCount, value);
            }
        }

        #endregion

        #region InjectTestingErrors Property

        private bool injectTestingErrors = false;

        /// <summary>
        ///     Gets or sets whether or not to inject testing errors in the serial connection test.
        /// </summary>
        public bool InjectTestingErrors
        {
            get { return injectTestingErrors; }
            set
            {
                // return true on change
                SetField(ref injectTestingErrors, value);
            }
        }

        #endregion

        #region HeightText Property

        private string heightText = "250";

        /// <summary>
        ///     Gets or sets the text of the image height.
        /// </summary>
        public string HeightText
        {
            get { return heightText; }
            set
            {
                // return true on change
                SetField(ref heightText, value);
            }
        }

        #endregion

        #region WidthText Property

        private string widthText = "1";

        /// <summary>
        ///     Gets or sets the text of the image width.
        /// </summary>
        public string WidthText
        {
            get { return widthText; }
            set
            {
                // return true on change
                SetField(ref widthText, value);
            }
        }

        #endregion

        #region DesiredBaudRateText Property

        private string desiredBaudRateText = "";

        /// <summary>
        ///     Gets or sets the desired baud rate text.
        /// </summary>
        public string DesiredBaudRateText
        {
            get { return desiredBaudRateText; }
            set
            {
                // return true on change
                if (SetField(ref desiredBaudRateText, value))
                {
                    ComputeRateParameters(Device.PicClockSpeed);
                    var setting = Double.Parse(Rates[0].Setting);
                    baudDivisorText = String.Format("{0:F1}", setting);
                    NotifyPropertyChanged(() => BaudDivisorText);
                }
            }
        }

        private void ComputeRateParameters(int picClockSpeed)
        {
            int rate;
            if (Int32.TryParse(DesiredBaudRateText, out rate))
            {
                Rates.Clear();
                Rates.Add(BaudRateCalculator.Compute(SerialDeviceType.PIC32, rate, picClockSpeed));
                Rates.Add(BaudRateCalculator.Compute(SerialDeviceType.FtdiLow, rate));
                Rates.Add(BaudRateCalculator.Compute(SerialDeviceType.FtdiHigh, rate));
                ActualBaudRateText = String.Format("{0:F1}", Rates[0].ActualBaud);
                ErrorRateText = String.Format("{0:F1}", Rates[0].PercentError);
            }
        }

        #endregion

        #region ActualBaudRateText Property

        private string actualBaudRateText = "";

        /// <summary>
        ///     Gets or sets property description...
        /// </summary>
        public string ActualBaudRateText
        {
            get { return actualBaudRateText; }
            set
            {
                // return true on change
                SetField(ref actualBaudRateText, value);
            }
        }

        #endregion

        #region ErrorRateText Property

        private string errorRateText = "";

        /// <summary>
        ///     Gets or sets the error rate text.
        /// </summary>
        public string ErrorRateText
        {
            get { return errorRateText; }
            set
            {
                // return true on change
                SetField(ref errorRateText, value);
            }
        }

        #endregion

        #region BaudDivisorText Property

        private string baudDivisorText = "";

        /// <summary>
        ///     Gets or sets the baud rate divisor text.
        /// </summary>
        public string BaudDivisorText
        {
            get { return baudDivisorText; }
            set
            {
                // return true on change
                if (!SetField(ref baudDivisorText, value)) return;

                int divisor;
                if (!Int32.TryParse(value, out divisor))
                    return; // do not set on illegal values
                baudDivisorText = value;

                var desiredRate = Device.PicClockSpeed/(4*(divisor + 1));
                DesiredBaudRateText = String.Format("{0}", desiredRate);
                ComputeRateParameters(Device.PicClockSpeed);
            }
        }

        #endregion

        #region SelectedSpeed Property

        private BaudRateSettings selectedSpeed = null;

        /// <summary>
        ///     Gets or sets the selected speed item.
        /// </summary>
        public BaudRateSettings SelectedSpeed
        {
            get { return selectedSpeed; }
            set
            {
                // return true on change
                if (!SetField(ref selectedSpeed, value)) return;
                DesiredBaudRateText = selectedSpeed.TargetRate.ToString();
                ActualBaudRateText = String.Format("{0:F1}", selectedSpeed.ActualBaud);
                ErrorRateText = String.Format("{0:F1}", selectedSpeed.PercentError);
                BaudDivisorText = selectedSpeed.Setting;
            }
        }

        #endregion

        #region RepeatByteText Property

        private string repeatByteText = "1";

        /// <summary>
        ///     Gets or sets the text for the number of repeats to send.
        /// </summary>
        public string RepeatByteText
        {
            get { return repeatByteText; }
            set
            {
                // return true on change
                SetField(ref repeatByteText, value);
            }
        }

        #endregion

        #region SendByteText Property

        private string sendByteText = "255 0 0 254";

        /// <summary>
        ///     Gets or sets the bytes to send.
        /// </summary>
        public string SendByteText
        {
            get { return sendByteText; }
            set
            {
                // return true on change
                SetField(ref sendByteText, value);
            }
        }

        #endregion

        #region BlueColorValue Property

        private double blueColorValue = 255.0;

        /// <summary>
        ///     Gets or sets the blue color value.
        /// </summary>
        public double BlueColorValue
        {
            get { return blueColorValue; }
            set
            {
                // return true on change
                if (SetField(ref blueColorValue, value))
                    ColorChanged();
            }
        }

        #endregion

        #region GreenColorValue Property

        private double greenColorValue = 255.0;

        /// <summary>
        ///     Gets or sets the green color.
        /// </summary>
        public double GreenColorValue
        {
            get { return greenColorValue; }
            set
            {
                // return true on change
                if (SetField(ref greenColorValue, value))
                    ColorChanged();
            }
        }

        #endregion

        #region RedColorValue Property

        private double redColorValue = 255.0;

        /// <summary>
        ///     Gets or sets the red color value.
        /// </summary>
        public double RedColorValue
        {
            get { return redColorValue; }
            set
            {
                // return true on change
                if (SetField(ref redColorValue, value))
                    ColorChanged();
            }
        }

        #endregion

        #region ThroughputFps Property

        private double throughputFps = 0.0;

        /// <summary>
        ///     Gets or sets the throughout frames per second.
        /// </summary>
        public double ThroughputFps
        {
            get { return throughputFps; }
            set
            {
                // return true on change
                SetField(ref throughputFps, value);
            }
        }

        #endregion

        #region PixelSliderValue Property

        private double pixelSliderValue = 0;

        /// <summary>
        ///     Gets or sets the value of the pixel slider test.
        /// </summary>
        public double PixelSliderValue
        {
            get { return pixelSliderValue; }
            set
            {
                // return true on change
                if (SetField(ref pixelSliderValue, value))
                    PixelSliderUpdate();
            }
        }

        #endregion

        #region IsConnectedAndNotDrawing Property

        private bool isConnectedAndNotDrawing = false;

        /// <summary>
        ///     Gets or sets if this is connected and not drawing.
        /// </summary>
        public bool IsConnectedAndNotDrawing
        {
            get { return isConnectedAndNotDrawing; }
            set
            {
                // return true on change
                SetField(ref isConnectedAndNotDrawing, value);
            }
        }

        #endregion

        #region DiagnosticLedState

        private bool diagnosticLedState = true;

        /// <summary>
        ///     Gets or sets the state of the diagnostic LED.
        /// </summary>
        public bool DiagnosticLedState
        {
            get { return diagnosticLedState; }
            set
            {
                // return true on change
                if (SetField(ref diagnosticLedState, value))
                    Device.SetLedUsage(diagnosticLedState);
            }
        }

        #endregion

        #region DemoDelayText Property

        private string demoDelayText = "10";

        /// <summary>
        ///     Gets or sets the demo delay text.
        /// </summary>
        public string DemoDelayText
        {
            get { return demoDelayText; }
            set
            {
                // return true on change
                SetField(ref demoDelayText, value);
            }
        }

        #endregion

        #region SkewDelay1 Property

        private int skewDelay1 = 20;

        /// <summary>
        ///     Gets or sets the timing skew delay.
        /// </summary>
        public int SkewDelay1
        {
            get { return skewDelay1; }
            set
            {
                // return true if there was a change.
                SetField(ref skewDelay1, value);
            }
        }

        #endregion

        #region SkewDelay2 Property

        private int skewDelay2 = 20;

        /// <summary>
        ///     Gets or sets the skew delay 2.
        /// </summary>
        public int SkewDelay2
        {
            get { return skewDelay2; }
            set
            {
                // return true if there was a change.
                SetField(ref skewDelay2, value);
            }
        }

        #endregion

        #region SkewDelay3 Property

        private int skewDelay3 = 20;

        /// <summary>
        ///     Gets or sets the delay skew 3.
        /// </summary>
        public int SkewDelay3
        {
            get { return skewDelay3; }
            set
            {
                // return true if there was a change.
                SetField(ref skewDelay3, value);
            }
        }

        #endregion

        #region IdBufferText Property

        private string idBufferText = "0";

        /// <summary>
        ///     Gets or sets the id string.
        /// </summary>
        public string IdBufferText
        {
            get { return idBufferText; }
            set
            {
                // return true on change
                SetField(ref idBufferText, value);
            }
        }

        #endregion


        #region DemoIndexText Property

        private string demoIndexText = "0";

        /// <summary>
        ///     Gets or sets the demo index.
        /// </summary>
        public string DemoIndexText
        {
            get { return demoIndexText; }
            set { SetField(ref demoIndexText, value); }
        }

        #endregion

        #region DemoLengthText Property

        private string demoLengthText = "5000";

        /// <summary>
        ///     Gets or sets the demo length in milliseconds.
        /// </summary>
        public string DemoLengthText
        {
            get { return demoLengthText; }
            set
            {
                // return true on change
                SetField(ref demoLengthText, value);
            }
        }

        #endregion
    }
}