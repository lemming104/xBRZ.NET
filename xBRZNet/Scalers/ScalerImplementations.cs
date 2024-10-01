namespace xBRZNet.Scalers
{
    using xBRZNet.Common;

    internal interface IScaler
    {
        int Scale { get; }
        void BlendLineSteep(uint col, OutputMatrix out_);
        void BlendLineSteepAndShallow(uint col, OutputMatrix out_);
        void BlendLineShallow(uint col, OutputMatrix out_);
        void BlendLineDiagonal(uint col, OutputMatrix out_);
        void BlendCorner(uint col, OutputMatrix out_);
    }

    internal abstract class ScalerBase
    {
        protected static void AlphaBlend(int n, int m, IntPtr dstPtr, uint col)
        {
            //assert n < 256 : "possible overflow of (col & redMask) * N";
            //assert m < 256 : "possible overflow of (col & redMask) * N + (dst & redMask) * (M - N)";
            //assert 0 < n && n < m : "0 < N && N < M";

            //this works because 8 upper bits are free
            uint dst = dstPtr.Get();
            uint redComponent = BlendComponent(Mask.Red, n, m, dst, col);
            uint greenComponent = BlendComponent(Mask.Green, n, m, dst, col);
            uint blueComponent = BlendComponent(Mask.Blue, n, m, dst, col);
            uint alphaComponent = Mask.Alpha & 0xffffffff;
            uint blend = redComponent | greenComponent | blueComponent | alphaComponent;
            dstPtr.Set(blend);
        }

        private static uint BlendComponent(uint mask, int n, int m, uint inPixel, uint setPixel)
        {
            uint inChan = inPixel & mask;
            uint setChan = setPixel & mask;
            long blend = (setChan * n) + (inChan * (m - n));
            uint component = unchecked((uint)(mask & (blend / m)));
            return component;
        }
    }

    internal class Scaler2X : ScalerBase, IScaler
    {
        public int Scale { get; } = 2;

        public void BlendLineShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(3, 4, out_.Ref(this.Scale - 1, 1), col);
        }

        public void BlendLineSteep(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(3, 4, out_.Ref(1, this.Scale - 1), col);
        }

        public void BlendLineSteepAndShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(1, 0), col);
            AlphaBlend(1, 4, out_.Ref(0, 1), col);
            AlphaBlend(5, 6, out_.Ref(1, 1), col); //[!] fixes 7/8 used in xBR
        }

        public void BlendLineDiagonal(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 2, out_.Ref(1, 1), col);
        }

        public void BlendCorner(uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(21, 100, out_.Ref(1, 1), col); //exact: 1 - pi/4 = 0.2146018366
        }
    }

    internal class Scaler3X : ScalerBase, IScaler
    {
        public int Scale { get; } = 3;

        public void BlendLineShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(3, 4, out_.Ref(this.Scale - 1, 1), col);
            out_.Ref(this.Scale - 1, 2).Set(col);
        }

        public void BlendLineSteep(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(3, 4, out_.Ref(1, this.Scale - 1), col);
            out_.Ref(2, this.Scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(2, 0), col);
            AlphaBlend(1, 4, out_.Ref(0, 2), col);
            AlphaBlend(3, 4, out_.Ref(2, 1), col);
            AlphaBlend(3, 4, out_.Ref(1, 2), col);
            out_.Ref(2, 2).Set(col);
        }

        public void BlendLineDiagonal(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 8, out_.Ref(1, 2), col);
            AlphaBlend(1, 8, out_.Ref(2, 1), col);
            AlphaBlend(7, 8, out_.Ref(2, 2), col);
        }

        public void BlendCorner(uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(45, 100, out_.Ref(2, 2), col); //exact: 0.4545939598
                                                      //alphaBlend(14, 1000, out.ref(2, 1), col); //0.01413008627 -> negligable
                                                      //alphaBlend(14, 1000, out.ref(1, 2), col); //0.01413008627
        }
    }

    internal class Scaler4X : ScalerBase, IScaler
    {
        public int Scale { get; } = 4;

        public void BlendLineShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(3, 4, out_.Ref(this.Scale - 1, 1), col);
            AlphaBlend(3, 4, out_.Ref(this.Scale - 2, 3), col);
            out_.Ref(this.Scale - 1, 2).Set(col);
            out_.Ref(this.Scale - 1, 3).Set(col);
        }

        public void BlendLineSteep(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(3, 4, out_.Ref(1, this.Scale - 1), col);
            AlphaBlend(3, 4, out_.Ref(3, this.Scale - 2), col);
            out_.Ref(2, this.Scale - 1).Set(col);
            out_.Ref(3, this.Scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(3, 4, out_.Ref(3, 1), col);
            AlphaBlend(3, 4, out_.Ref(1, 3), col);
            AlphaBlend(1, 4, out_.Ref(3, 0), col);
            AlphaBlend(1, 4, out_.Ref(0, 3), col);
            AlphaBlend(1, 3, out_.Ref(2, 2), col); //[!] fixes 1/4 used in xBR
            out_.Ref(3, 3).Set(col);
            out_.Ref(3, 2).Set(col);
            out_.Ref(2, 3).Set(col);
        }

        public void BlendLineDiagonal(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 2, out_.Ref(this.Scale - 1, this.Scale / 2), col);
            AlphaBlend(1, 2, out_.Ref(this.Scale - 2, (this.Scale / 2) + 1), col);
            out_.Ref(this.Scale - 1, this.Scale - 1).Set(col);
        }

        public void BlendCorner(uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(68, 100, out_.Ref(3, 3), col); //exact: 0.6848532563
            AlphaBlend(9, 100, out_.Ref(3, 2), col); //0.08677704501
            AlphaBlend(9, 100, out_.Ref(2, 3), col); //0.08677704501
        }
    }

    internal class Scaler5X : ScalerBase, IScaler
    {
        public int Scale { get; } = 5;

        public void BlendLineShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(1, 4, out_.Ref(this.Scale - 3, 4), col);
            AlphaBlend(3, 4, out_.Ref(this.Scale - 1, 1), col);
            AlphaBlend(3, 4, out_.Ref(this.Scale - 2, 3), col);
            out_.Ref(this.Scale - 1, 2).Set(col);
            out_.Ref(this.Scale - 1, 3).Set(col);
            out_.Ref(this.Scale - 1, 4).Set(col);
            out_.Ref(this.Scale - 2, 4).Set(col);
        }

        public void BlendLineSteep(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(1, 4, out_.Ref(4, this.Scale - 3), col);
            AlphaBlend(3, 4, out_.Ref(1, this.Scale - 1), col);
            AlphaBlend(3, 4, out_.Ref(3, this.Scale - 2), col);
            out_.Ref(2, this.Scale - 1).Set(col);
            out_.Ref(3, this.Scale - 1).Set(col);
            out_.Ref(4, this.Scale - 1).Set(col);
            out_.Ref(4, this.Scale - 2).Set(col);
        }

        public void BlendLineSteepAndShallow(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(3, 4, out_.Ref(1, this.Scale - 1), col);
            AlphaBlend(1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(3, 4, out_.Ref(this.Scale - 1, 1), col);
            out_.Ref(2, this.Scale - 1).Set(col);
            out_.Ref(3, this.Scale - 1).Set(col);
            out_.Ref(this.Scale - 1, 2).Set(col);
            out_.Ref(this.Scale - 1, 3).Set(col);
            out_.Ref(4, this.Scale - 1).Set(col);
            AlphaBlend(2, 3, out_.Ref(3, 3), col);
        }

        public void BlendLineDiagonal(uint col, OutputMatrix out_)
        {
            AlphaBlend(1, 8, out_.Ref(this.Scale - 1, this.Scale / 2), col);
            AlphaBlend(1, 8, out_.Ref(this.Scale - 2, (this.Scale / 2) + 1), col);
            AlphaBlend(1, 8, out_.Ref(this.Scale - 3, (this.Scale / 2) + 2), col);
            AlphaBlend(7, 8, out_.Ref(4, 3), col);
            AlphaBlend(7, 8, out_.Ref(3, 4), col);
            out_.Ref(4, 4).Set(col);
        }

        public void BlendCorner(uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(86, 100, out_.Ref(4, 4), col); //exact: 0.8631434088
            AlphaBlend(23, 100, out_.Ref(4, 3), col); //0.2306749731
            AlphaBlend(23, 100, out_.Ref(3, 4), col); //0.2306749731
                                                      //alphaBlend(8, 1000, out.ref(4, 2), col); //0.008384061834 -> negligable
                                                      //alphaBlend(8, 1000, out.ref(2, 4), col); //0.008384061834
        }
    }
}
