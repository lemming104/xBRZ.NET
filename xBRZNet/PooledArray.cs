namespace xBRZNet
{
    using System;
    using System.Buffers;

    /// <summary>
    /// An array from a shared buffer pool
    /// </summary>
    /// <typeparam name="T">Data type of the array</typeparam>
    public class PooledArray<T> : IDisposable
    {
        private static readonly ArrayPool<T> BufferPool = ArrayPool<T>.Shared;

        private bool m_disposedValue;
        private readonly Action m_disposeAction;

        /// <summary>
        /// Array data
        /// </summary>
        public readonly T[] Data;

        /// <summary>
        /// The size that was requested when the array was allocated.  This may be smaller than the actual size.
        /// </summary>
        public int RequestedSize { get; }

        /// <summary>
        /// Obtains a new array of at least the specified size
        /// </summary>
        /// <param name="size">Desired array size.  Note that the returned array may be larger.</param>
        public PooledArray(int size)
        {
            this.RequestedSize = size;
            this.Data = BufferPool.Rent(size);
            this.m_disposeAction = () => BufferPool.Return(this.Data);
        }

        /// <summary>
        /// Returns the array data to its buffer pool
        /// </summary>
        /// <param name="disposing"></param>
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

        /// <summary>
        /// Disposes this object, returning the array data to its originating buffer pool
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
