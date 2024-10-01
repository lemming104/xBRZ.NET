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

        internal readonly uint[] Data;

        private readonly PooledArray<uint> m_dataPooled;

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
            this.m_dataPooled = new PooledArray<uint>(width * height);
            this.Data = this.m_dataPooled.Data;
        }

        /// <summary>
        /// Converts the image from bytes
        /// </summary>
        /// <param name="imageBytes">Source image bytesr</param>
        /// <exception cref="ArgumentException">Thrown if byte array is too small for specified dimensions</exception>
        public unsafe void FromRgba(byte[] imageBytes)
        {
            if (imageBytes.Length < this.Width * this.Height * 4)
            {
                throw new ArgumentException("Data does not match dimensions", nameof(imageBytes));
            }

            fixed (uint* imageDataFixed = this.Data)
            {
                byte* dataBytes = (byte*)imageDataFixed;

                int size = this.Width * this.Height * 4;
                for (int i = 0; i < size; i++)
                {
                    dataBytes[i] = imageBytes[i];
                }
            }
        }

        /// <summary>
        /// Converts the image to bytes for output or further manipulation
        /// </summary>
        /// <returns>A pooled array containing the image in byte form</returns>
        public unsafe PooledArray<byte> ToBytes()
        {
            PooledArray<byte> retArray = new PooledArray<byte>(this.Data.Length * 4);
            byte[] outputBytes = retArray.Data;

            fixed (uint* imageDataFixed = this.Data)
            {
                byte* dataBytes = (byte*)imageDataFixed;

                int size = this.Width * this.Height * 4;
                for (int i = 0; i < size; i++)
                {
                    outputBytes[i] = dataBytes[i];
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
