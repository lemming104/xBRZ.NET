namespace xBRZNet
{
    using xBRZNet.Common;

    /// <summary>
    /// Scaler configuration parameters.  Note that <see cref="ScalerCfg.Default"/> provides reasonable defaults and uses ARGB color.
    /// </summary>
    public class ScalerCfg
    {
        /// <summary>
        /// Luminance weight
        /// </summary>
        public double LuminanceWeight { get; set; } = 1;

        /// <summary>
        /// Equal-color tolerance
        /// </summary>
        public double EqualColorTolerance { get; set; } = 30;

        /// <summary>
        /// Dominant direction detection threshold
        /// </summary>
        public double DominantDirectionThreshold { get; set; } = 3.6;

        /// <summary>
        /// Steep direction threshold
        /// </summary>
        public double SteepDirectionThreshold { get; set; } = 2.2;

        private ColorMode m_colorMode = ColorMode.ARGB;

        /// <summary>
        /// Color mode
        /// </summary>
        public ColorMode ColorMode
        {
            get
            {
                return this.m_colorMode;
            }
            set
            {
                this.m_colorMode = value;
                this.Mask = new Mask(value);
            }
        }

        internal Mask Mask { get; set; } = new Mask(ColorMode.ARGB);

        /// <summary>
        /// Returns a <see cref="ScalerCfg"/> with default values
        /// </summary>
        public static ScalerCfg Default = new ScalerCfg();
    }
}
