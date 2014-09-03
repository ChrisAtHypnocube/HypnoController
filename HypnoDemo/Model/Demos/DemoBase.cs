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
using System.IO;
using System.Windows.Documents;

namespace Hypnocube.Demo.Model.Demos
{
    public class DemoBase : Surface
    {
        // provide random numbers to all subclasses.
        public static Random Rand = new Random();

        protected int Frame = 0;

        public DemoBase(int w, int h) : base(w, h)
        {
        }

        /// <summary>
        ///     Called to restart the demo
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// A string containing parameters for this demo
        /// </summary>
        public string ParameterText { get; set; }

        /// <summary>
        /// Description of the parameter
        /// </summary>
        public string ParameterDescription { get; protected set; }


        /// <summary>
        /// If the parameter text is of the form
        /// separator char, then entries, returns the number of parameters
        /// </summary>
        /// <returns></returns>
        protected int ParameterCount()
        {
            return Parameters().Count;
        }

        /// <summary>
        /// If the parameter text is of the form
        /// separator char, then entries, returns the number of parameters
        /// </summary>
        /// <returns></returns>
        protected List<string> Parameters()
        {
            var ans = new List<string>();
            if (!string.IsNullOrEmpty(ParameterText))
            {
                var sep = ParameterText[0];
                var words = ParameterText.Split(new[] {sep});
                ans.AddRange(words);
            }
            return ans;
        }

        // draw the next frame
        public virtual void Update()
        {
            ++Frame;
        }
    }
}