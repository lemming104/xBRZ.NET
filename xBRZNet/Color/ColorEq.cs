namespace xBRZNet.Color
{
    internal class ColorEq : ColorDist
    {
        public ColorEq(ScalerCfg cfg) : base(cfg) { }

        public bool IsColorEqual(int color1, int color2)
        {
            double eqColorThres = this.Cfg.EqualColorTolerance * this.Cfg.EqualColorTolerance;
            return this.DistYCbCr(color1, color2) < eqColorThres;
        }
    }
}
