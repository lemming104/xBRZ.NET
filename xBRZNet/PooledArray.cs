namespace xBRZNet
{
    using System;
    using System.Buffers;

    public class PooledArray<T> : IDisposable
    {
        private static readonly ArrayPool<T> BufferPool = ArrayPool<T>.Shared;

        private bool m_disposedValue;
        private readonly Action m_disposeAction;

        public T[] Data;

        public PooledArray(int size)
        {
            this.Data = BufferPool.Rent(size);
            this.m_disposeAction = () => BufferPool.Return(this.Data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_disposedValue)
            {
                if (disposing)
                {
                    this.m_disposeAction();
                }

                this.m_disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
