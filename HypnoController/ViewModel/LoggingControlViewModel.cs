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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Hypnocube.MVVM;

namespace Hypnocube.HypnoController.ViewModel
{
    public class LoggingControlViewModel : ViewModelBase, IMessage
    {
        public LoggingControlViewModel()
        {
            BinaryMessages = new ObservableCollection<BinaryLine>();
            ClearLogCommand = new RelayCommand(o => ClearLog());
            CopyLogCommand = new RelayCommand(o => CopyLog());
            Messages = new ObservableCollection<string>();
        }

        /// <summary>
        ///     Messages to view
        /// </summary>
        public ObservableCollection<string> Messages { get; private set; }

        /// <summary>
        ///     Formatted binary data
        /// </summary>
        public ObservableCollection<BinaryLine> BinaryMessages { get; private set; }

        public ICommand ClearLogCommand { get; private set; }

        public ICommand CopyLogCommand { get; private set; }

        #region IsLoggingEnabled Property
        private bool isLoggingEnabled = true;
        /// <summary>
        /// Gets or sets if the logging is on.
        /// </summary>
        public bool IsLoggingEnabled
        {
            get { return isLoggingEnabled; }
            set
            {
                // return true if there was a change.
                SetField(ref isLoggingEnabled, value);
            }
        }
        #endregion

        #region CurrentLine Property
        private string currentLine = "";
        /// <summary>
        /// Gets or sets the current line.
        /// </summary>
        public string CurrentLine
        {
            get { return currentLine; }
            private set
            {
                // return true if there was a change.
                SetField(ref currentLine, value);
            }
        }
        #endregion


        void AddMessageInternal(string message)
        {
            Messages.Add(message);
            CurrentLine = message;
        }

        #region IMessage Members

        public void AddError(string message)
        {
            // todo - color, blinking control, etc?
            if (isLoggingEnabled)
                Dispatch(()=>AddMessageInternal("ERROR: " + message));
        }

        public void AddMessage(string message)
        {
            if (isLoggingEnabled)
                Dispatch(() => AddMessageInternal(message));
        }

        public void AddMessage(byte[] data, bool asBinary)
        {
            if (isLoggingEnabled)
            {
                if (asBinary)
                    Dispatch(() => FormatBinaryData(data));
                else
                    Dispatch(() => FormatData(data));
            }
        }

        #endregion

        private void FormatData(byte[] data)
        {
            var lastIndex = Messages.Count;
            var sb = new StringBuilder();
            if (lastIndex > 0)
            {
                sb.Append(Messages[lastIndex - 1]);
                Messages.RemoveAt(lastIndex - 1);
            }
            foreach (var b in data)
            {
                if (b == '\n')
                {
                    AddMessageInternal(sb.ToString());
                    sb.Clear();
                }
                else if (b != '\r')
                {
                    if ((b >= 20) && (b < 128))
                        sb.Append((char) b);
                    else
                    {
                        sb.AppendFormat("[0x{0:X2}]", (int) b);
                    }
                }
            }
            AddMessageInternal(sb.ToString());

            //if (Messages.Count > 0)
            //    logBox.ScrollIntoView(Messages[Messages.Count - 1]);
        }


        private void FormatBinaryData(byte[] data)
        {
            var lastIndex = BinaryMessages.Count;
            var current = new BinaryLine();
            if (lastIndex > 0 && BinaryMessages[lastIndex - 1].Data.Count < 16)
            {
                current = BinaryMessages.Last();
                BinaryMessages.RemoveAt(lastIndex - 1);
            }
            foreach (var b in data)
            {
                current.Data.Add(b);
                if (current.Data.Count == 16)
                {
                    BinaryMessages.Add(current);
                    current = new BinaryLine();
                }
            }
            if (current.Data.Count > 0)
                BinaryMessages.Add(current);
        }

        private void ClearLog()
        {
            Messages.Clear();
            BinaryMessages.Clear();
        }

        private void CopyLog()
        {
            var sb = new StringBuilder();
            foreach (var item in Messages)
                sb.Append(item + Environment.NewLine);
            Clipboard.SetText(sb.ToString());
        }
    }
}