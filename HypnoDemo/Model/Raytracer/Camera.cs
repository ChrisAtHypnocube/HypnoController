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
using Hypnocube.Demo.Model.Raytracer.Math3D;

namespace Hypnocube.Demo.Model.Raytracer
{
    internal class Camera
    {
        private readonly Vector3D cameraDirection;
        private readonly Vector3D cameraRight;

        public Camera(Vector3D eyePoint, Vector3D aimPoint, Vector3D upDirection)
        {
            EyePoint = new Vector3D(eyePoint);
            AimPoint = new Vector3D(aimPoint);
            UpDirection = new Vector3D(upDirection);
            UpDirection.Normalize();
            ImageScale = 1;

            cameraDirection = (aimPoint - eyePoint).Normalize();
            cameraRight = Vector3D.Cross(cameraDirection, upDirection);
            UpDirection = Vector3D.Cross(cameraRight, cameraDirection); // helps with slop
        }

        public Vector3D EyePoint { get; private set; }
        public Vector3D AimPoint { get; private set; }
        public Vector3D UpDirection { get; private set; }

        /// <summary>
        ///     used to effectively move the render plane farther away
        /// </summary>
        public double ImageScale { get; set; }

        /// <summary>
        ///     Given a location, compute the ray to send
        ///     normalizedHorizontal goes -0.5 to 0.5 in width of image
        ///     normalizedVertical goes -0.5 to 0.5 in height of image
        ///     note for non-square images, the delta in each direction per pixel should be the same
        /// </summary>
        /// <param name="normalizedHorizontal"></param>
        /// <param name="normalizedVertical"></param>
        /// <returns></returns>
        public Ray ComputeCameraRay(double normalizedHorizontal, double normalizedVertical)
        {
            var imagePoint = normalizedHorizontal*cameraRight +
                             normalizedVertical*UpDirection +
                             EyePoint + cameraDirection;
            imagePoint *= ImageScale;
            var rayDir = imagePoint - EyePoint;
            return new Ray(EyePoint, rayDir);
        }
    }
}