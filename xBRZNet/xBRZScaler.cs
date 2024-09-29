namespace xBRZNet
{
    using System;
    using xBRZNet.Blend;
    using xBRZNet.Color;
    using xBRZNet.Common;
    using xBRZNet.Scalers;

    //http://intrepidis.blogspot.com/2014/02/xbrz-in-java.html
    /*
        -------------------------------------------------------------------------
        | xBRZ: "Scale by rules" - high quality image upscaling filter by Zenju |
        -------------------------------------------------------------------------
        using a modified approach of xBR:
        http://board.byuu.org/viewtopic.php?f=10&t=2248
        - new rule set preserving small image features
        - support multithreading
        - support 64 bit architectures
        - support processing image slices
    */

    /*
        -> map source (src.Width * src.Height) to target (scale * width x scale * height)
        image, optionally processing a half-open slice of rows [yFirst, yLast) only
        -> color format: ARGB (BGRA char order), alpha channel unused
        -> support for source/target pitch in chars!
        -> if your emulator changes only a few image slices during each cycle
        (e.g. Dosbox) then there's no need to run xBRZ on the complete image:
        Just make sure you enlarge the source image slice by 2 rows on top and
        2 on bottom (this is the additional range the xBRZ algorithm is using
        during analysis)
        Caveat: If there are multiple changed slices, make sure they do not overlap
        after adding these additional rows in order to avoid a memory race condition 
        if you are using multiple threads for processing each enlarged slice!

        THREAD-SAFETY: - parts of the same image may be scaled by multiple threads
        as long as the [yFirst, yLast) ranges do not overlap!
        - there is a minor inefficiency for the first row of a slice, so avoid
        processing single rows only
        */

    /*
        Converted to Java 7 by intrepidis. It would have been nice to use
        Java 8 lambdas, but Java 7 is more ubiquitous at the time of writing,
        so this code uses anonymous local classes instead.
        Regarding multithreading, each thread should have its own instance
        of the xBRZ class.
    */

    // ReSharper disable once InconsistentNaming
    public class xBRZScaler
    {
        private readonly ScalerCfg _cfg;
        private readonly IScaler _scaler;
        private OutputMatrix? _outputMatrix;
        private readonly BlendResult _blendResult = new BlendResult();

        private readonly ColorDist _colorDistance;
        private readonly ColorEq _colorEqualizer;

        static xBRZScaler()
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new PlatformNotSupportedException("Not supported on big-endian systems.");
            }
        }

        public xBRZScaler(int scaleSize, ScalerCfg cfg)
        {
            this._scaler = scaleSize.ToIScaler();
            this._cfg = cfg;
            this._colorDistance = new ColorDist(this._cfg);
            this._colorEqualizer = new ColorEq(this._cfg);
        }

        //fill block with the given color
        private static void FillBlock(int[] trg, int trgi, int pitch, int col, int blockSize)
        {
            for (int y = 0; y < blockSize; ++y, trgi += pitch)
            {
                for (int x = 0; x < blockSize; ++x)
                {
                    trg[trgi + x] = col;
                }
            }
        }

        //detect blend direction
        private void PreProcessCorners(Kernel4x4 ker)
        {
            this._blendResult.Reset();

            if ((ker.F == ker.G && ker.J == ker.K) || (ker.F == ker.J && ker.G == ker.K)) return;

            ColorDist dist = this._colorDistance;

            const int weight = 4;
            double jg = dist.DistYCbCr(ker.I, ker.F) + dist.DistYCbCr(ker.F, ker.C) + dist.DistYCbCr(ker.N, ker.K) + dist.DistYCbCr(ker.K, ker.H) + (weight * dist.DistYCbCr(ker.J, ker.G));
            double fk = dist.DistYCbCr(ker.E, ker.J) + dist.DistYCbCr(ker.J, ker.O) + dist.DistYCbCr(ker.B, ker.G) + dist.DistYCbCr(ker.G, ker.L) + (weight * dist.DistYCbCr(ker.F, ker.K));

            if (jg < fk)
            {
                bool dominantGradient = this._cfg.DominantDirectionThreshold * jg < fk;
                if (ker.F != ker.G && ker.F != ker.J)
                {
                    this._blendResult.F = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
                if (ker.K != ker.J && ker.K != ker.G)
                {
                    this._blendResult.K = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
            }
            else if (fk < jg)
            {
                bool dominantGradient = this._cfg.DominantDirectionThreshold * fk < jg;
                if (ker.J != ker.F && ker.J != ker.K)
                {
                    this._blendResult.J = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
                if (ker.G != ker.F && ker.G != ker.K)
                {
                    this._blendResult.G = (char)(dominantGradient ? BlendType.Dominant : BlendType.Normal);
                }
            }
        }

        /*
            input kernel area naming convention:
            -------------
            | A | B | C |
            ----|---|---|
            | D | E | F | //input pixel is at position E
            ----|---|---|
            | G | H | I |
            -------------
            blendInfo: result of preprocessing all four corners of pixel "e"
        */
        private void ScalePixel(IScaler scaler, int rotDeg, Kernel3x3 ker, int trgi, char blendInfo)
        {
            // int a = ker._[Rot._[(0 << 2) + rotDeg]];
            int b = ker._[Rot._[(1 << 2) + rotDeg]];
            int c = ker._[Rot._[(2 << 2) + rotDeg]];
            int d = ker._[Rot._[(3 << 2) + rotDeg]];
            int e = ker._[Rot._[(4 << 2) + rotDeg]];
            int f = ker._[Rot._[(5 << 2) + rotDeg]];
            int g = ker._[Rot._[(6 << 2) + rotDeg]];
            int h = ker._[Rot._[(7 << 2) + rotDeg]];
            int i = ker._[Rot._[(8 << 2) + rotDeg]];

            char blend = blendInfo.Rotate((RotationDegree)rotDeg);

            if ((BlendType)blend.GetBottomR() == BlendType.None) return;

            ColorEq eq = this._colorEqualizer;
            ColorDist dist = this._colorDistance;

            bool doLineBlend;

#pragma warning disable IDE0045

            if (blend.GetBottomR() >= (char)BlendType.Dominant)
            {
                doLineBlend = true;
            }
            //make sure there is no second blending in an adjacent
            //rotation for this pixel: handles insular pixels, mario eyes
            //but support double-blending for 90� corners
            else if (blend.GetTopR() != (char)BlendType.None && !eq.IsColorEqual(e, g))
            {
                doLineBlend = false;
            }
            else if (blend.GetBottomL() != (char)BlendType.None && !eq.IsColorEqual(e, c))
            {
                doLineBlend = false;
            }
            //no full blending for L-shapes; blend corner only (handles "mario mushroom eyes")
            else if (eq.IsColorEqual(g, h) && eq.IsColorEqual(h, i) && eq.IsColorEqual(i, f) && eq.IsColorEqual(f, c) && !eq.IsColorEqual(e, i))
            {
                doLineBlend = false;
            }
            else
            {
                doLineBlend = true;
            }

#pragma warning restore ID0045

            //choose most similar color
            int px = dist.DistYCbCr(e, f) <= dist.DistYCbCr(e, h) ? f : h;

            OutputMatrix? out_ = this._outputMatrix;
            out_!.Move(rotDeg, trgi);

            if (!doLineBlend)
            {
                scaler.BlendCorner(px, out_);
                return;
            }

            //test sample: 70% of values max(fg, hc) / min(fg, hc)
            //are between 1.1 and 3.7 with median being 1.9
            double fg = dist.DistYCbCr(f, g);
            double hc = dist.DistYCbCr(h, c);

            bool haveShallowLine = this._cfg.SteepDirectionThreshold * fg <= hc && e != g && d != g;
            bool haveSteepLine = this._cfg.SteepDirectionThreshold * hc <= fg && e != c && b != c;

            if (haveShallowLine)
            {
                if (haveSteepLine)
                {
                    scaler.BlendLineSteepAndShallow(px, out_);
                }
                else
                {
                    scaler.BlendLineShallow(px, out_);
                }
            }
            else
            {
                if (haveSteepLine)
                {
                    scaler.BlendLineSteep(px, out_);
                }
                else
                {
                    scaler.BlendLineDiagonal(px, out_);
                }
            }
        }

        /// <summary>
        /// Scale an image
        /// </summary>
        /// <param name="src">Source image, color format: ARGB (BGRA char order)</param>
        /// <param name="trg">Target image</param>
        public void ScaleImage(Image src, Image trg)
        {
            this.ScaleImage(src, trg, 0, src.Height);
        }

        /// <param name="src.Width">Source image width</param>
        /// <param name="src.Height">Source image height</param>
        /// <param name="yFirst">First row to process</param>
        /// <param name="yLast">Last row to process</param>
        public void ScaleImage(Image src, Image trg, int yFirst, int yLast)
        {
            yFirst = Math.Max(yFirst, 0);
            yLast = Math.Min(yLast, src.Height);

            if (yFirst >= yLast)
            {
                throw new ArgumentException("yLast must be greater than yFirst", nameof(yLast));
            }

            if (src.Width < 0)
            {
                throw new ArgumentException("Width must be greater than zero", nameof(src.Width));
            }

            int trgWidth = src.Width * this._scaler.Scale;
            int minTrgSize = trgWidth * (yLast - yFirst) * this._scaler.Scale;
            if (trg.Data.Length < minTrgSize)
            {
                throw new ArgumentException("Destination array not large enough", nameof(trg));
            }

            //temporary buffer for "on the fly preprocessing"
            char[] preProcBuffer = new char[src.Width];

            Kernel4x4 ker4 = new Kernel4x4();

            //initialize preprocessing buffer for first row:
            //detect upper left and right corner blending
            //this cannot be optimized for adjacent processing
            //stripes; we must not allow for a memory race condition!
            if (yFirst > 0)
            {
                int y = yFirst - 1;

                int sM1 = src.Width * Math.Max(y - 1, 0);
                int s0 = src.Width * y; //center line
                int sP1 = src.Width * Math.Min(y + 1, src.Height - 1);
                int sP2 = src.Width * Math.Min(y + 2, src.Height - 1);

                for (int x = 0; x < src.Width; ++x)
                {
                    int xM1 = Math.Max(x - 1, 0);
                    int xP1 = Math.Min(x + 1, src.Width - 1);
                    int xP2 = Math.Min(x + 2, src.Width - 1);

                    //read sequentially from memory as far as possible
                    ker4.A = src.Data[sM1 + xM1];
                    ker4.B = src.Data[sM1 + x];
                    ker4.C = src.Data[sM1 + xP1];
                    ker4.D = src.Data[sM1 + xP2];

                    ker4.E = src.Data[s0 + xM1];
                    ker4.F = src.Data[s0 + x];
                    ker4.G = src.Data[s0 + xP1];
                    ker4.H = src.Data[s0 + xP2];

                    ker4.I = src.Data[sP1 + xM1];
                    ker4.J = src.Data[sP1 + x];
                    ker4.K = src.Data[sP1 + xP1];
                    ker4.L = src.Data[sP1 + xP2];

                    ker4.M = src.Data[sP2 + xM1];
                    ker4.N = src.Data[sP2 + x];
                    ker4.O = src.Data[sP2 + xP1];
                    ker4.P = src.Data[sP2 + xP2];

                    this.PreProcessCorners(ker4); // writes to blendResult
                    /*
                    preprocessing blend result:
                    ---------
                    | F | G | //evalute corner between F, G, J, K
                    ----|---| //input pixel is at position F
                    | J | K |
                    ---------
                    */
                    preProcBuffer[x] = preProcBuffer[x].SetTopR(this._blendResult.J);

                    if (x + 1 < src.Width)
                    {
                        preProcBuffer[x + 1] = preProcBuffer[x + 1].SetTopL(this._blendResult.K);
                    }
                }
            }

            this._outputMatrix = new OutputMatrix(this._scaler.Scale, trg.Data, trgWidth);

            Kernel3x3 ker3 = new Kernel3x3();

            for (int y = yFirst; y < yLast; ++y)
            {
                //consider MT "striped" access
                int trgi = this._scaler.Scale * y * trgWidth;

                int sM1 = src.Width * Math.Max(y - 1, 0);
                int s0 = src.Width * y; //center line
                int sP1 = src.Width * Math.Min(y + 1, src.Height - 1);
                int sP2 = src.Width * Math.Min(y + 2, src.Height - 1);

                char blendXy1 = (char)0;

                for (int x = 0; x < src.Width; ++x, trgi += this._scaler.Scale)
                {
                    int xM1 = Math.Max(x - 1, 0);
                    int xP1 = Math.Min(x + 1, src.Width - 1);
                    int xP2 = Math.Min(x + 2, src.Width - 1);

                    //evaluate the four corners on bottom-right of current pixel
                    //blend_xy for current (x, y) position

                    //read sequentially from memory as far as possible
                    ker4.A = src.Data[sM1 + xM1];
                    ker4.B = src.Data[sM1 + x];
                    ker4.C = src.Data[sM1 + xP1];
                    ker4.D = src.Data[sM1 + xP2];

                    ker4.E = src.Data[s0 + xM1];
                    ker4.F = src.Data[s0 + x];
                    ker4.G = src.Data[s0 + xP1];
                    ker4.H = src.Data[s0 + xP2];

                    ker4.I = src.Data[sP1 + xM1];
                    ker4.J = src.Data[sP1 + x];
                    ker4.K = src.Data[sP1 + xP1];
                    ker4.L = src.Data[sP1 + xP2];

                    ker4.M = src.Data[sP2 + xM1];
                    ker4.N = src.Data[sP2 + x];
                    ker4.O = src.Data[sP2 + xP1];
                    ker4.P = src.Data[sP2 + xP2];

                    this.PreProcessCorners(ker4); // writes to blendResult

                    /*
                        preprocessing blend result:
                        ---------
                        | F | G | //evaluate corner between F, G, J, K
                        ----|---| //current input pixel is at position F
                        | J | K |
                        ---------
                    */

                    //all four corners of (x, y) have been determined at
                    //this point due to processing sequence!
                    char blendXy = preProcBuffer[x].SetBottomR(this._blendResult.F);

                    //set 2nd known corner for (x, y + 1)
                    blendXy1 = blendXy1.SetTopR(this._blendResult.J);
                    //store on current buffer position for use on next row
                    preProcBuffer[x] = blendXy1;

                    //set 1st known corner for (x + 1, y + 1) and
                    //buffer for use on next column
                    blendXy1 = ((char)0).SetTopL(this._blendResult.K);

                    if (x + 1 < src.Width)
                    {
                        //set 3rd known corner for (x + 1, y)
                        preProcBuffer[x + 1] = preProcBuffer[x + 1].SetBottomL(this._blendResult.G);
                    }

                    //fill block of size scale * scale with the given color
                    //  //place *after* preprocessing step, to not overwrite the
                    //  //results while processing the the last pixel!
                    FillBlock(trg.Data, trgi, trgWidth, src.Data[s0 + x], this._scaler.Scale);

                    //blend four corners of current pixel
                    if (blendXy == 0) continue;

                    const int a = 0, b = 1, c = 2, d = 3, e = 4, f = 5, g = 6, h = 7, i = 8;

                    //read sequentially from memory as far as possible
                    ker3._[a] = src.Data[sM1 + xM1];
                    ker3._[b] = src.Data[sM1 + x];
                    ker3._[c] = src.Data[sM1 + xP1];

                    ker3._[d] = src.Data[s0 + xM1];
                    ker3._[e] = src.Data[s0 + x];
                    ker3._[f] = src.Data[s0 + xP1];

                    ker3._[g] = src.Data[sP1 + xM1];
                    ker3._[h] = src.Data[sP1 + x];
                    ker3._[i] = src.Data[sP1 + xP1];

                    this.ScalePixel(this._scaler, (int)RotationDegree.R0, ker3, trgi, blendXy);
                    this.ScalePixel(this._scaler, (int)RotationDegree.R90, ker3, trgi, blendXy);
                    this.ScalePixel(this._scaler, (int)RotationDegree.R180, ker3, trgi, blendXy);
                    this.ScalePixel(this._scaler, (int)RotationDegree.R270, ker3, trgi, blendXy);
                }
            }
        }
    }
}
