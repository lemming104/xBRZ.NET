namespace xBRZTester
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.PixelFormats;
    using System;
    using System.Buffers;
    using System.Runtime.InteropServices;

    public class Converters
    {
        private static ArrayPool<uint> m_byteArrayPool = ArrayPool<uint>.Shared;

        public class ArgbImagePooled : xBRZNet.Image, IDisposable
        {
            private bool disposedValue;
            private readonly ArrayPool<uint>? m_sourceArrayPool;

            public ArgbImagePooled(ArrayPool<uint> pool, uint width, uint height)
                : base(pool.Rent((int)(width * height)), width, height)
            {
                m_sourceArrayPool = pool;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        m_sourceArrayPool?.Return(Data);
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

        public static ArgbImagePooled GetEmptyImage(uint width, uint height)
        {
            return new ArgbImagePooled(m_byteArrayPool, width, height);
        }

        public static ArgbImagePooled LoadImageArgb(string fileName, out uint width, out uint height)
        {
            using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(fileName))
            {
                width = (uint)image.Width;
                height = (uint)image.Height;

                ArgbImagePooled argbImage = new ArgbImagePooled(m_byteArrayPool, width, height);

                uint offset = 0;
                for (int row = 0; row < height; row++)
                {
                    Span<Rgba32> pixelRow = image.DangerousGetPixelRowMemory(row).Span;
                    foreach (ref Rgba32 pixel in pixelRow)
                    {
                        argbImage.Data[offset] = pixel.Rgba;
                    }
                }

                return argbImage;
            }
        }

        public unsafe static void WriteImageArgb(Span<uint> argbData, uint width, uint height, string fileName)
        {
            fixed (uint* argbDataFixed = argbData)
            {
                byte* argbBytes = (byte*)argbDataFixed;
                ReadOnlySpan<byte> byteSpan = MemoryMarshal.CreateReadOnlySpan<byte>(ref argbBytes[0], (int)(width * height * 4));

                using (Image image = Image.LoadPixelData<Rgba32>(byteSpan, (int)width, (int)height))
                {
                    image.SaveAsPng(fileName);
                }
            }
        }
    }
}
