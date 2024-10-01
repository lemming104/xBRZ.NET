namespace xBRZNet.Scalers
{
    using xBRZNet.Common;

    internal interface IScaler
    {
        int Scale { get; }
        void BlendLineSteep(Mask mask, uint col, OutputMatrix out_);
        void BlendLineSteepAndShallow(Mask mask, uint col, OutputMatrix out_);
        void BlendLineShallow(Mask mask, uint col, OutputMatrix out_);
        void BlendLineDiagonal(Mask mask, uint col, OutputMatrix out_);
        void BlendCorner(Mask mask, uint col, OutputMatrix out_);
    }

    internal abstract class ScalerBase
    {
        protected static void AlphaBlend(Mask mask, int n, int m, IntPtr dstPtr, uint col)
        {
            uint dst = dstPtr.Get();
            uint redComponent = BlendComponent(mask.Red, n, m, dst, col);
            uint greenComponent = BlendComponent(mask.Green, n, m, dst, col);
            uint blueComponent = BlendComponent(mask.Blue, n, m, dst, col);
            uint alphaComponent = mask.Alpha & 0xffffffff;
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

        public void BlendLineShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(mask, 3, 4, out_.Ref(this.Scale - 1, 1), col);
        }

        public void BlendLineSteep(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(mask, 3, 4, out_.Ref(1, this.Scale - 1), col);
        }

        public void BlendLineSteepAndShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(1, 0), col);
            AlphaBlend(mask, 1, 4, out_.Ref(0, 1), col);
            AlphaBlend(mask, 5, 6, out_.Ref(1, 1), col); //[!] fixes 7/8 used in xBR
        }

        public void BlendLineDiagonal(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 2, out_.Ref(1, 1), col);
        }

        public void BlendCorner(Mask mask, uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(mask, 21, 100, out_.Ref(1, 1), col); //exact: 1 - pi/4 = 0.2146018366
        }
    }

    internal class Scaler3X : ScalerBase, IScaler
    {
        public int Scale { get; } = 3;

        public void BlendLineShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(mask, 3, 4, out_.Ref(this.Scale - 1, 1), col);
            out_.Ref(this.Scale - 1, 2).Set(col);
        }

        public void BlendLineSteep(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(mask, 1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(mask, 3, 4, out_.Ref(1, this.Scale - 1), col);
            out_.Ref(2, this.Scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(2, 0), col);
            AlphaBlend(mask, 1, 4, out_.Ref(0, 2), col);
            AlphaBlend(mask, 3, 4, out_.Ref(2, 1), col);
            AlphaBlend(mask, 3, 4, out_.Ref(1, 2), col);
            out_.Ref(2, 2).Set(col);
        }

        public void BlendLineDiagonal(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 8, out_.Ref(1, 2), col);
            AlphaBlend(mask, 1, 8, out_.Ref(2, 1), col);
            AlphaBlend(mask, 7, 8, out_.Ref(2, 2), col);
        }

        public void BlendCorner(Mask mask, uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(mask, 45, 100, out_.Ref(2, 2), col); //exact: 0.4545939598
                                                            //AlphaBlend(mask, 14, 1000, out.ref(2, 1), col); //0.01413008627 -> negligable
                                                            //AlphaBlend(mask, 14, 1000, out.ref(1, 2), col); //0.01413008627
        }
    }

    internal class Scaler4X : ScalerBase, IScaler
    {
        public int Scale { get; } = 4;

        public void BlendLineShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(mask, 3, 4, out_.Ref(this.Scale - 1, 1), col);
            AlphaBlend(mask, 3, 4, out_.Ref(this.Scale - 2, 3), col);
            out_.Ref(this.Scale - 1, 2).Set(col);
            out_.Ref(this.Scale - 1, 3).Set(col);
        }

        public void BlendLineSteep(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(mask, 1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(mask, 3, 4, out_.Ref(1, this.Scale - 1), col);
            AlphaBlend(mask, 3, 4, out_.Ref(3, this.Scale - 2), col);
            out_.Ref(2, this.Scale - 1).Set(col);
            out_.Ref(3, this.Scale - 1).Set(col);
        }

        public void BlendLineSteepAndShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 3, 4, out_.Ref(3, 1), col);
            AlphaBlend(mask, 3, 4, out_.Ref(1, 3), col);
            AlphaBlend(mask, 1, 4, out_.Ref(3, 0), col);
            AlphaBlend(mask, 1, 4, out_.Ref(0, 3), col);
            AlphaBlend(mask, 1, 3, out_.Ref(2, 2), col); //[!] fixes 1/4 used in xBR
            out_.Ref(3, 3).Set(col);
            out_.Ref(3, 2).Set(col);
            out_.Ref(2, 3).Set(col);
        }

        public void BlendLineDiagonal(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 2, out_.Ref(this.Scale - 1, this.Scale / 2), col);
            AlphaBlend(mask, 1, 2, out_.Ref(this.Scale - 2, (this.Scale / 2) + 1), col);
            out_.Ref(this.Scale - 1, this.Scale - 1).Set(col);
        }

        public void BlendCorner(Mask mask, uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(mask, 68, 100, out_.Ref(3, 3), col); //exact: 0.6848532563
            AlphaBlend(mask, 9, 100, out_.Ref(3, 2), col); //0.08677704501
            AlphaBlend(mask, 9, 100, out_.Ref(2, 3), col); //0.08677704501
        }
    }

    internal class Scaler5X : ScalerBase, IScaler
    {
        public int Scale { get; } = 5;

        public void BlendLineShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 3, 4), col);
            AlphaBlend(mask, 3, 4, out_.Ref(this.Scale - 1, 1), col);
            AlphaBlend(mask, 3, 4, out_.Ref(this.Scale - 2, 3), col);
            out_.Ref(this.Scale - 1, 2).Set(col);
            out_.Ref(this.Scale - 1, 3).Set(col);
            out_.Ref(this.Scale - 1, 4).Set(col);
            out_.Ref(this.Scale - 2, 4).Set(col);
        }

        public void BlendLineSteep(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(mask, 1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(mask, 1, 4, out_.Ref(4, this.Scale - 3), col);
            AlphaBlend(mask, 3, 4, out_.Ref(1, this.Scale - 1), col);
            AlphaBlend(mask, 3, 4, out_.Ref(3, this.Scale - 2), col);
            out_.Ref(2, this.Scale - 1).Set(col);
            out_.Ref(3, this.Scale - 1).Set(col);
            out_.Ref(4, this.Scale - 1).Set(col);
            out_.Ref(4, this.Scale - 2).Set(col);
        }

        public void BlendLineSteepAndShallow(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 4, out_.Ref(0, this.Scale - 1), col);
            AlphaBlend(mask, 1, 4, out_.Ref(2, this.Scale - 2), col);
            AlphaBlend(mask, 3, 4, out_.Ref(1, this.Scale - 1), col);
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 1, 0), col);
            AlphaBlend(mask, 1, 4, out_.Ref(this.Scale - 2, 2), col);
            AlphaBlend(mask, 3, 4, out_.Ref(this.Scale - 1, 1), col);
            out_.Ref(2, this.Scale - 1).Set(col);
            out_.Ref(3, this.Scale - 1).Set(col);
            out_.Ref(this.Scale - 1, 2).Set(col);
            out_.Ref(this.Scale - 1, 3).Set(col);
            out_.Ref(4, this.Scale - 1).Set(col);
            AlphaBlend(mask, 2, 3, out_.Ref(3, 3), col);
        }

        public void BlendLineDiagonal(Mask mask, uint col, OutputMatrix out_)
        {
            AlphaBlend(mask, 1, 8, out_.Ref(this.Scale - 1, this.Scale / 2), col);
            AlphaBlend(mask, 1, 8, out_.Ref(this.Scale - 2, (this.Scale / 2) + 1), col);
            AlphaBlend(mask, 1, 8, out_.Ref(this.Scale - 3, (this.Scale / 2) + 2), col);
            AlphaBlend(mask, 7, 8, out_.Ref(4, 3), col);
            AlphaBlend(mask, 7, 8, out_.Ref(3, 4), col);
            out_.Ref(4, 4).Set(col);
        }

        public void BlendCorner(Mask mask, uint col, OutputMatrix out_)
        {
            //model a round corner
            AlphaBlend(mask, 86, 100, out_.Ref(4, 4), col); //exact: 0.8631434088
            AlphaBlend(mask, 23, 100, out_.Ref(4, 3), col); //0.2306749731
            AlphaBlend(mask, 23, 100, out_.Ref(3, 4), col); //0.2306749731
                                                            //AlphaBlend(mask, 8, 1000, out.ref(4, 2), col); //0.008384061834 -> negligable
                                                            //AlphaBlend(mask, 8, 1000, out.ref(2, 4), col); //0.008384061834
        }
    }
}
