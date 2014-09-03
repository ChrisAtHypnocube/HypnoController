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
using System.IO;

namespace Hypnocube
{
    public static class FileTools
    {

        /// <summary>
        /// Get the current executing assembly path
        /// </summary>
        /// <returns></returns>
        public static string AssemblyPath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
        /// <summary>
        /// Get a location in the user directory to store files
        /// Requires company and application names
        /// </summary>
        /// <returns></returns>
        public static string GetUserStoragePath(string companyName, string applicationName)
        {
            if (String.IsNullOrEmpty(companyName))
                throw new ArgumentException("GetUserStoragePath needs company name","companyName");
            if (String.IsNullOrEmpty(companyName))
                throw new ArgumentException("GetUserStoragePath needs application name","applicationName");

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // ensure this part available
            var companyPath = Path.Combine(path, companyName);
            if (!Directory.Exists(companyPath))
                Directory.CreateDirectory(companyPath);
            if (!Directory.Exists(companyPath))
                throw new DirectoryNotFoundException("Could not create directory " + companyPath);

            // ensure this part available
            var appPath = Path.Combine(companyPath,applicationName);
            if (!Directory.Exists(appPath))
                Directory.CreateDirectory(appPath);
            if (!Directory.Exists(appPath))
                throw new DirectoryNotFoundException("Could not create directory " + appPath);
            return appPath;
        }
    }
}
