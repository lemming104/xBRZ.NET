namespace xBRZNet.Scalers
{
    using xBRZNet.Common;

    //access matrix area, top-left at position "out" for image with given width
    internal class OutputMatrix
    {
        private readonly IntPtr _out;
        private readonly int _outWidth;
        private readonly int _n;
        private int _outi;
        private int _nr;

        private const int MaxScale = 5; // Highest possible scale
        private const int MaxScaleSquared = MaxScale * MaxScale;

        public OutputMatrix(int scale, int[] outPtr, int outWidth)
        {
            this._n = (scale - 2) * Rot.MaxRotations * MaxScaleSquared;
            this._out = new IntPtr(outPtr);
            this._outWidth = outWidth;
        }

        public void Move(int rotDeg, int outi)
        {
            this._nr = this._n + (rotDeg * MaxScaleSquared);
            this._outi = outi;
        }

        public IntPtr Ref(int i, int j)
        {
            IntPair rot = MatrixRotation[this._nr + (i * MaxScale) + j];
            this._out.Position(this._outi + rot.J + (rot.I * this._outWidth));
            return this._out;
        }

        //calculate input matrix coordinates after rotation at program startup
        private static readonly IntPair[] MatrixRotation = new IntPair[(MaxScale - 1) * MaxScaleSquared * Rot.MaxRotations];

        static OutputMatrix()
        {
            for (int n = 2; n < MaxScale + 1; n++)
            {
                for (int r = 0; r < Rot.MaxRotations; r++)
                {
                    int nr = ((n - 2) * Rot.MaxRotations * MaxScaleSquared) + (r * MaxScaleSquared);
                    for (int i = 0; i < MaxScale; i++)
                    {
                        for (int j = 0; j < MaxScale; j++)
                        {
                            MatrixRotation[nr + (i * MaxScale) + j] = BuildMatrixRotation(r, i, j, n);
                        }
                    }
                }
            }
        }

        private static IntPair BuildMatrixRotation(int rotDeg, int i, int j, int n)
        {
            int iOld, jOld;

            if (rotDeg == 0)
            {
                iOld = i;
                jOld = j;
            }
            else
            {
                //old coordinates before rotation!
                IntPair old = BuildMatrixRotation(rotDeg - 1, i, j, n);
                iOld = n - 1 - old.J;
                jOld = old.I;
            }

            return new IntPair(iOld, jOld);
        }
    }
}
