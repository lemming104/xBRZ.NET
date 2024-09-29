namespace xBRZNet
{
    using System;

    /// <summary>
    /// Represents an image stored in a pooled buffer
    /// </summary>
    public class Image : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Width of the image, in pixels
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Height of the image, in pixels
        /// </summary>
        public readonly int Height;

        internal readonly int[] Data;

        private readonly PooledArray<int> m_dataPooled;

        /// <summary>
        /// Allocate a new Image of the specified size
        /// </summary>
        /// <param name="width">Width of the image, in pixels</param>
        /// <param name="height">Height of the image, in pixels</param>
        /// <exception cref="ArgumentException">Thrown if width or height are invalid</exception>
        public Image(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentException("Width must be greater than zero", nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentException("Height must be greater than zero", nameof(height));
            }

            this.Width = width;
            this.Height = height;
            this.m_dataPooled = new PooledArray<int>(width * height);
            this.Data = this.m_dataPooled.Data;
        }

        /// <summary>
        /// Converts the image from bytes in an A R G B ordering
        /// </summary>
        /// <param name="argbBytes">Source image bytes in A R G B order</param>
        /// <exception cref="ArgumentException">Thrown if byte array is too small for specified dimensions</exception>
        public unsafe void FromArgb(byte[] argbBytes)
        {
            if (argbBytes.Length < this.Width * this.Height * 4)
            {
                throw new ArgumentException("Data does not match dimensions", nameof(argbBytes));
            }

            fixed (int* imageDataFixed = this.Data)
            {
                byte* dataBytes = (byte*)imageDataFixed;

                // Convert into B G R A byte ordering
                int size = this.Width * this.Height;
                for (int i = 0; i < size; i++)
                {
                    int baseAddr = i * 4;
                    dataBytes[baseAddr] = argbBytes[baseAddr + 3];
                    dataBytes[baseAddr + 1] = argbBytes[baseAddr + 2];
                    dataBytes[baseAddr + 2] = argbBytes[baseAddr + 1];
                    dataBytes[baseAddr + 3] = argbBytes[baseAddr];
                }
            }
        }

        /// <summary>
        /// Converts the image from bytes in an R G B A ordering
        /// </summary>
        /// <param name="rgbaBytes">Source image bytes in R G B A order</param>
        /// <exception cref="ArgumentException">Thrown if byte array is too small for specified dimensions</exception>
        public unsafe void FromRgba(byte[] rgbaBytes)
        {
            if (rgbaBytes.Length < this.Width * this.Height * 4)
            {
                throw new ArgumentException("Data does not match dimensions", nameof(rgbaBytes));
            }

            fixed (int* imageDataFixed = this.Data)
            {
                byte* dataBytes = (byte*)imageDataFixed;

                // Convert into B G R A byte ordering
                int size = this.Width * this.Height;
                for (int i = 0; i < size; i++)
                {
                    int baseAddr = i * 4;
                    dataBytes[baseAddr] = rgbaBytes[baseAddr + 2];
                    dataBytes[baseAddr + 1] = rgbaBytes[baseAddr + 1];
                    dataBytes[baseAddr + 2] = rgbaBytes[baseAddr];
                    dataBytes[baseAddr + 3] = rgbaBytes[baseAddr + 3];
                }
            }
        }

        /// <summary>
        /// Converts the image to A R G B byte ordering for output or further manipulation
        /// </summary>
        /// <returns>A pooled array containing an A R G B representation of the image</returns>
        public unsafe PooledArray<byte> ToArgb()
        {
            PooledArray<byte> retArray = new PooledArray<byte>(this.Data.Length * 4);
            byte[] argbBytes = retArray.Data;

            fixed (int* imageDataFixed = this.Data)
            {
                byte* dataBytes = (byte*)imageDataFixed;

                int size = this.Width * this.Height;
                for (int i = 0; i < size; i++)
                {
                    int baseAddr = i * 4;
                    argbBytes[baseAddr] = dataBytes[baseAddr + 3];
                    argbBytes[baseAddr + 1] = dataBytes[baseAddr + 2];
                    argbBytes[baseAddr + 2] = dataBytes[baseAddr + 1];
                    argbBytes[baseAddr + 3] = dataBytes[baseAddr];
                }
            }

            return retArray;
        }

        /// <summary>
        /// Converts the image to R G B A byte ordering for output or further manipulation
        /// </summary>
        /// <returns>A pooled array containing an R G B A representation of the image</returns>
        public unsafe PooledArray<byte> ToRgba()
        {
            PooledArray<byte> retArray = new PooledArray<byte>(this.Data.Length * 4);
            byte[] rgbaBytes = retArray.Data;

            fixed (int* imageDataFixed = this.Data)
            {
                byte* dataBytes = (byte*)imageDataFixed;

                int size = this.Width * this.Height;
                for (int i = 0; i < size; i++)
                {
                    int baseAddr = i * 4;
                    rgbaBytes[baseAddr] = dataBytes[baseAddr + 2];
                    rgbaBytes[baseAddr + 1] = dataBytes[baseAddr + 1];
                    rgbaBytes[baseAddr + 2] = dataBytes[baseAddr];
                    rgbaBytes[baseAddr + 3] = dataBytes[baseAddr + 3];
                }
            }

            return retArray;
        }

        /// <summary>
        /// Returns the array data to its buffer pool
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.m_dataPooled.Dispose();
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes this object, returning the array data to its originating buffer pool
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
