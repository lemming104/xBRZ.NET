namespace xBRZNet.Color
{
    internal class ColorDist
    {
        protected readonly ScalerCfg Cfg;

        public ColorDist(ScalerCfg cfg)
        {
            this.Cfg = cfg;
        }

        public double DistYCbCr(uint pix1, uint pix2)
        {
            if (pix1 == pix2) return 0;

            //http://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
            //YCbCr conversion is a matrix multiplication => take advantage of linearity by subtracting first!
            int rDiff = (unchecked((int)(pix1 & this.Cfg.Mask.Red)) - unchecked((int)(pix2 & this.Cfg.Mask.Red))) >> this.Cfg.Mask.RedShiftBits; //we may delay division by 255 to after matrix multiplication
            int gDiff = (unchecked((int)(pix1 & this.Cfg.Mask.Green)) - unchecked((int)(pix2 & this.Cfg.Mask.Green))) >> this.Cfg.Mask.GreenShiftBits;
            int bDiff = (unchecked((int)(pix1 & this.Cfg.Mask.Blue)) - unchecked((int)(pix2 & this.Cfg.Mask.Blue))) >> this.Cfg.Mask.BlueShiftBits; //subtraction for int is noticeable faster than for double

            const double kB = 0.0722; //ITU-R BT.709 conversion
            const double kR = 0.2126;
            const double kG = 1 - kB - kR;

            const double scaleB = 0.5 / (1 - kB);
            const double scaleR = 0.5 / (1 - kR);

            double y = (kR * rDiff) + (kG * gDiff) + (kB * bDiff); //[!], analog YCbCr!
            double cB = scaleB * (bDiff - y);
            double cR = scaleR * (rDiff - y);

            // Skip division by 255.
            // Also skip square root here by pre-squaring the config option equalColorTolerance.
            double yWeighted = this.Cfg.LuminanceWeight * y;

            return (yWeighted * yWeighted) + (cB * cB) + (cR * cR);
        }
    }
}
