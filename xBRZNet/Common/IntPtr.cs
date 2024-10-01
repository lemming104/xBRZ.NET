namespace xBRZNet.Common
{
    internal class IntPtr
    {
        private readonly uint[] _array;
        private int _ptr;

        public IntPtr(uint[] array)
        {
            this._array = array;
        }

        public void Position(int position)
        {
            this._ptr = position;
        }

        public uint Get()
        {
            return this._array[this._ptr];
        }

        public void Set(uint val)
        {
            this._array[this._ptr] = val;
        }
    }
}
