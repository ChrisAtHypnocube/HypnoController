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

/* TODO - needs
 * 1. Requirements, such as "connected" or "drawing mode" or such. Enum of all such is needed.
 * 2. When able to be shown, based on required components on?
 * 3. Ability to store text
 * 4. How to save state (combo boxes saving all would be good, ability to clear them, or max entries)
 * 5. What parameters mean (ability to pass images, etc, around?)
 * 
 */

namespace Hypnocube
{
    /// <summary>
    ///     Represent a feature such as a command or property in one of the hypnocube control objects
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute()
        {
            Label = "unknown";
            Help = "unknown";
            GroupName = "unknown";
            MinValue = Int32.MinValue;
            MaxValue = Int32.MaxValue;
        }

        /// <summary>
        ///     A label used when this command appears in a GUI
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        ///     Help text, such as a tooltip
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        ///     A name where all items are grouped in the GUI
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        ///     If present, minimum legal value
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        ///     If present, maximum legal value
        /// </summary>
        public int MaxValue { get; set; }
    }
}