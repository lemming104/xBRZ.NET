namespace xBRZNet.Scalers
{
    using System;
    using System.Linq;

    internal static class Rot
    {
        public const int MaxRotations = 4; // Number of 90 degree rotations
        public const int MaxPositions = 9;

        // Cache the 4 rotations of the 9 positions, a to i.
        // a = 0, b = 1, c = 2,
        // d = 3, e = 4, f = 5,
        // g = 6, h = 7, i = 8;
        public static int[] _ { get; } = new int[MaxRotations * MaxPositions];

        static Rot()
        {
            int[] rotation = Enumerable.Range(0, MaxPositions).ToArray();
            int sideLength = (int)Math.Sqrt(MaxPositions);
            for (int rot = 0; rot < MaxRotations; rot++)
            {
                for (int pos = 0; pos < MaxPositions; pos++)
                {
                    _[(pos * MaxRotations) + rot] = rotation[pos];
                }
                rotation = rotation.RotateClockwise(sideLength);
            }
        }

        //http://stackoverflow.com/a/38964502/294804
        private static int[] RotateClockwise(this int[] square1DMatrix, int? sideLength = null)
        {
            int size = sideLength ?? (int)Math.Sqrt(square1DMatrix.Length);
            int[] result = new int[square1DMatrix.Length];

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    result[(i * size) + j] = square1DMatrix[((size - j - 1) * size) + i];
                }
            }

            return result;
        }
    }
}
