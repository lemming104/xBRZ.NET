namespace xBRZNet.Blend
{
    internal class BlendResult
    {
        public char F { get; set; }
        public char G { get; set; }
        public char J { get; set; }
        public char K { get; set; }

        public void Reset()
        {
            this.F = this.G = this.J = this.K = (char)0;
        }
    }
}
