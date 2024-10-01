namespace xBRZNet.Color
{
    internal class ColorEq : ColorDist
    {
        public ColorEq(ScalerCfg cfg) : base(cfg) { }

        public bool IsColorEqual(uint color1, uint color2)
        {
            double eqColorThres = this.Cfg.EqualColorTolerance * this.Cfg.EqualColorTolerance;
            return this.DistYCbCr(color1, color2) < eqColorThres;
        }
    }
}
