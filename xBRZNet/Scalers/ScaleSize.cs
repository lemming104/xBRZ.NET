namespace xBRZNet.Scalers
{
    using System.Linq;

    internal static class ScaleSize
    {
        private static readonly IScaler[] Scalers =
        {
            new Scaler2X(),
            new Scaler3X(),
            new Scaler4X(),
            new Scaler5X()
        };

        public static IScaler ToIScaler(this int scaleSize)
        {
            return scaleSize < 2 || scaleSize > 5
                ? throw new System.ArgumentException("Scale must be in the range 2-5", nameof(scaleSize))
                : Scalers.Single(s => s.Scale == scaleSize);
        }
    }
}
