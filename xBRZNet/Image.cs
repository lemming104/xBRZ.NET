namespace xBRZNet
{
    using System;
    using System.Buffers;

    public class Image : IDisposable
    {
        private static ArrayPool<int> IntArrayPool = ArrayPool<int>.Shared;
        private bool disposedValue;

        public readonly int Width;
        public readonly int Height;
        public readonly int[] Data;

        public Image(int width, int height)
        {
            if (width == 0)
            {
                throw new ArgumentException("Width must be greater than zero", nameof(width));
            }

            if (height == 0)
            {
                throw new ArgumentException("Height must be greater than zero", nameof(height));
            }

            Width = width;
            Height = height;
            Data = IntArrayPool.Rent(Width * Height);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    IntArrayPool.Return(Data);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
