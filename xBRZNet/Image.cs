namespace xBRZNet
{
    using System;

    public class Image : IDisposable
    {
        private bool disposedValue;

        public readonly int Width;
        public readonly int Height;
        internal readonly int[] Data;

        private readonly PooledArray<int> m_dataPooled;

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

            this.Width = width;
            this.Height = height;
            this.m_dataPooled = new PooledArray<int>(width * height);
            this.Data = this.m_dataPooled.Data;
        }

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

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
