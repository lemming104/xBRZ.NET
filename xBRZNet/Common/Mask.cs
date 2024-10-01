namespace xBRZNet.Common
{
    internal class Mask
    {
        public const uint Red = 0x00ff0000;
        public const uint Green = 0x0000ff00;
        public const uint Blue = 0x000000ff;
        public const uint Alpha = 0xff000000;

        public const int RedShiftBits = 16;
        public const int GreenShiftBits = 8;
        public const int BlueShiftBits = 0;
        public const int AlphaShiftBits = 24;
    }
}
