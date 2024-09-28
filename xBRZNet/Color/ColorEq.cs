namespace xBRZNet.Color
{
    using System;

    internal class ColorEq : ColorDist
    {
        public ColorEq(ScalerCfg cfg) : base(cfg) { }

        public bool IsColorEqual(int color1, int color2)
        {
            double eqColorThres = Math.Pow(this.Cfg.EqualColorTolerance, 2);
            return this.DistYCbCr(color1, color2) < eqColorThres;
        }
    }
}
