namespace xBRZNet.Common
{
    internal class IntPtr
    {
        private readonly int[] _array;
        private int _ptr;

        public IntPtr(int[] array)
        {
            this._array = array;
        }

        public void Position(int position)
        {
            this._ptr = position;
        }

        public int Get()
        {
            return this._array[this._ptr];
        }

        public void Set(int val)
        {
            this._array[this._ptr] = val;
        }
    }
}
