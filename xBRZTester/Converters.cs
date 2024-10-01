namespace xBRZTester
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.PixelFormats;
    using System;

    public class Converters
    {
        public static xBRZNet.Image LoadImageRGBA(string fileName, out int width, out int height)
        {
            using (Image image = Image.Load(fileName))
            {
                width = image.Width;
                height = image.Height;

                // Ensure Rgba32 color format
                using (Image<Rgba32> converted = image is Image<Rgba32>
                    ? (Image<Rgba32>)image
                    : image.CloneAs<Rgba32>())
                {
                    using (xBRZNet.PooledArray<byte> imageBytes = new xBRZNet.PooledArray<byte>(image.Width * image.Height * 4))
                    {
                        int offset = 0;
                        for (int row = 0; row < height; row++)
                        {
                            Span<Rgba32> pixelRow = converted.DangerousGetPixelRowMemory(row).Span;
                            foreach (ref Rgba32 pixel in pixelRow)
                            {
                                imageBytes.Data[offset] = pixel.R;
                                imageBytes.Data[offset + 1] = pixel.G;
                                imageBytes.Data[offset + 2] = pixel.B;
                                imageBytes.Data[offset + 3] = pixel.A;

                                offset += 4;
                            }
                        }

                        xBRZNet.Image outputImage = new xBRZNet.Image(width, height);
                        outputImage.FromBytes(imageBytes.Data);
                        return outputImage;
                    }
                }
            }
        }

        public static void WriteImageRGBA(xBRZNet.Image image, string fileName)
        {
            using (xBRZNet.PooledArray<byte> rgbaBytes = image.ToBytes())
            {
                Image<Rgba32> outputImage = Image.LoadPixelData<Rgba32>(rgbaBytes.Data, image.Width, image.Height);
                outputImage.SaveAsPng(fileName);
            }
        }
    }
}
