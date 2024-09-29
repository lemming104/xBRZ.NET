namespace xBRZNet
{
    /// <summary>
    /// Scaler configuration parameters.  Note that <see cref="ScalerCfg.Default"/> provides reasonable defaults./>
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

        /// <summary>
        /// Returns a <see cref="ScalerCfg"/> with default values
        /// </summary>
        public static ScalerCfg Default = new ScalerCfg();
    }
}
