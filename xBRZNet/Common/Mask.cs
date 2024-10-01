namespace xBRZNet.Common
{
    using System;

    /// <summary>
    /// Defines the mode used for color-related math
    /// </summary>
    public enum ColorMode
    {
        /// <summary>
        /// Byte ordering red-green-blue-alpha
        /// </summary>
        RGBA,
        /// <summary>
        /// Byte ordering alpha-red-green-blue
        /// </summary>
        ARGB
    }

    internal class Mask
    {
        public bool isColorModeSet = false;

        public uint Red = 0;
        public uint Green = 0;
        public uint Blue = 0;
        public uint Alpha = 0;

        public int RedShiftBits = 0;
        public int GreenShiftBits = 0;
        public int BlueShiftBits = 0;
        public int AlphaShiftBits = 0;

        internal Mask(ColorMode mode)
        {
            bool isLittleEndian = BitConverter.IsLittleEndian;
            if (isLittleEndian)
            {
                if (mode == ColorMode.ARGB)  // BGRA byte order
                {
                    this.Blue = 0xff000000;
                    this.Green = 0x00ff0000;
                    this.Red = 0x0000ff00;
                    this.Alpha = 0x000000ff;

                    this.BlueShiftBits = 24;
                    this.GreenShiftBits = 16;
                    this.RedShiftBits = 8;
                    this.AlphaShiftBits = 0;
                }
                else // RGBA -- ABGR byte order
                {
                    this.Alpha = 0xff000000;
                    this.Blue = 0x00ff0000;
                    this.Green = 0x0000ff00;
                    this.Red = 0x000000ff;

                    this.AlphaShiftBits = 24;
                    this.BlueShiftBits = 16;
                    this.GreenShiftBits = 8;
                    this.RedShiftBits = 0;
                }
            }
            else // Big-endian
            {
                if (mode == ColorMode.ARGB)
                {
                    this.Alpha = 0xff000000;
                    this.Red = 0x00ff0000;
                    this.Green = 0x0000ff00;
                    this.Blue = 0x000000ff;

                    this.AlphaShiftBits = 24;
                    this.RedShiftBits = 16;
                    this.GreenShiftBits = 8;
                    this.BlueShiftBits = 0;
                }
                else // RGBA
                {
                    this.Red = 0xff000000;
                    this.Green = 0x00ff0000;
                    this.Blue = 0x0000ff00;
                    this.Alpha = 0x000000ff;

                    this.RedShiftBits = 24;
                    this.GreenShiftBits = 16;
                    this.BlueShiftBits = 8;
                    this.AlphaShiftBits = 0;
                }
            }

            this.isColorModeSet = true;
        }
    }
}
