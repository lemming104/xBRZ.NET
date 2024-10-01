namespace xBRZNet.Scalers
{
    /*
        input kernel area naming convention:
        -----------------
        | A | B | C | D |
        ----|---|---|---|
        | E | F | G | H | //evalute the four corners between F, G, J, K
        ----|---|---|---| //input pixel is at position F
        | I | J | K | L |
        ----|---|---|---|
        | M | N | O | P |
        -----------------
    */
    // ReSharper disable once InconsistentNaming
    internal class Kernel4x4
    {
        public uint A, B, C, D;
        public uint E, F, G, H;
        public uint I, J, K, L;
        public uint M, N, O, P;
    }
}
