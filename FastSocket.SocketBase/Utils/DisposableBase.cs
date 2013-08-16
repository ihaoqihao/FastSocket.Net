using System;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// disposable base
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// finalize
        /// </summary>
        ~DisposableBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">true表示清理托管资源</param>
        private void Dispose(bool disposing)
        {
            if (this._disposed) return;
            this.Free(disposing);
            this._disposed = true;
        }

        /// <summary>
        /// free
        /// </summary>
        /// <param name="disposing"></param>
        protected abstract void Free(bool disposing);

        /// <summary>
        /// 检测并抛出<see cref="ObjectDisposedException"/>
        /// </summary>
        protected void CheckDisposedWithException()
        {
            if (this._disposed) throw new ObjectDisposedException(this.GetType().ToString());
        }
        /// <summary>
        /// true表示已disposed
        /// </summary>
        public bool IsDisposed
        {
            get { return this._disposed; }
        }
    }
}