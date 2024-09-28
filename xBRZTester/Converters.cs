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
        public static xBRZNet.Image LoadImageArgb(string fileName, out int width, out int height)
        {
            using (Image image = Image.Load(fileName))
            {
                width = image.Width;
                height = image.Height;

                xBRZNet.Image argbImage = new xBRZNet.Image(width, height);

                // Ensure Rgba32 color format
                using (Image<Rgba32> converted = image is Image<Rgba32>
                    ? (Image<Rgba32>)image
                    : image.CloneAs<Rgba32>())
                {

                    uint offset = 0;
                    for (int row = 0; row < height; row++)
                    {
                        Span<Rgba32> pixelRow = converted.DangerousGetPixelRowMemory(row).Span;
                        foreach (ref Rgba32 pixel in pixelRow)
                        {
                            argbImage.Data[offset] = unchecked((int)pixel.Rgba);
                            offset++;
                        }
                    }

                    return argbImage;
                }
            }
        }

        public unsafe static void WriteImageArgb(Span<int> argbData, int width, int height, string fileName)
        {
            fixed (int* argbDataFixed = argbData)
            {
                byte* argbBytes = (byte*)argbDataFixed;
                ReadOnlySpan<byte> byteSpan = MemoryMarshal.CreateReadOnlySpan<byte>(ref argbBytes[0], width * height * 4);

                using (Image image = Image.LoadPixelData<Rgba32>(byteSpan, width, height))
                {
                    image.SaveAsPng(fileName);
                }
            }
        }
    }
}
