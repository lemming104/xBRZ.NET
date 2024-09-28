namespace xBRZNet
{
    using System;

    public class Image
    {
        public readonly uint Width;
        public readonly uint Height;
        public readonly uint[] Data;

        public Image(uint[] data, uint width, uint height)
        {
            if (width == 0)
            {
                throw new ArgumentException("Width must be greater than zero", nameof(width));
            }

            if (height == 0)
            {
                throw new ArgumentException("Height must be greater than zero", nameof(height));
            }

            if (data.Length < width * height)
            {
                throw new ArgumentException("Data array must be greater than or equal to the product of width and height", nameof(data));
            }

            Data = data;
            Width = width;
            Height = height;
        }
    }
}
