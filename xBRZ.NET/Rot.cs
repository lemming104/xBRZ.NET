﻿using System;
using System.Linq;

namespace xBRZ.NET
{
    internal static class Rot
    {
        // Cache the 4 rotations of the 9 positions, a to i.
        // a = 0, b = 1, c = 2,
        // d = 3, e = 4, f = 5,
        // g = 6, h = 7, i = 8;
        public static int[] _ { get; } = new int[Common.MaxRotations * Common.MaxPositions];

        static Rot()
        {
            var rotation = Enumerable.Range(0, Common.MaxPositions).ToArray();
            var sideLength = (int) Math.Sqrt(Common.MaxPositions);
            for (var rot = 0; rot < Common.MaxRotations; rot++)
            {
                for (var pos = 0; pos < Common.MaxPositions; pos++)
                {
                    _[(pos * Common.MaxRotations) + rot] = rotation[pos];
                }
                rotation = rotation.RotateClockwise(sideLength);
            }
        }

        //http://stackoverflow.com/a/38964502/294804
        private static int[] RotateClockwise(this int[] square1DMatrix, int? sideLength = null)
        {
            var size = sideLength ?? (int)Math.Sqrt(square1DMatrix.Length);
            var result = new int[square1DMatrix.Length];

            for (var i = 0; i < size; ++i)
            {
                for (var j = 0; j < size; ++j)
                {
                    result[i * size + j] = square1DMatrix[(size - j - 1) * size + i];
                }
            }

            return result;
        }
    }
}
