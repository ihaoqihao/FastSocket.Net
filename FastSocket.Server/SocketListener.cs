using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// socket listener
    /// </summary>
    public sealed class SocketListener : ISocketListener
    {
        #region Private Members
        private readonly SocketBase.IHost _host = null;
        private const int BACKLOG = 500;
        private Socket _socket = null;
        private readonly SocketAsyncEventArgs _ae = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="host"></param>
        /// <exception cref="ArgumentNullException">endPoint is null</exception>
        /// <exception cref="ArgumentNullException">host is null</exception>
        public SocketListener(IPEndPoint endPoint, SocketBase.IHost host)
        {
            if (endPoint == null) throw new ArgumentNullException("endPoint");
            if (host == null) throw new ArgumentNullException("host");

            this.EndPoint = endPoint;
            this._host = host;

            this._ae = new SocketAsyncEventArgs();
            this._ae.Completed += this.AcceptCompleted;
        }
        #endregion

        #region ISocketListener Members
        /// <summary>
        /// socket accepted event
        /// </summary>
        public event Action<ISocketListener, SocketBase.IConnection> Accepted;
        /// <summary>
        /// get listener endPoint
        /// </summary>
        public EndPoint EndPoint { get; private set; }
        /// <summary>
        /// start
        /// </summary>
        public void Start()
        {
            if (this._socket == null)
            {
                this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._socket.Bind(this.EndPoint);
                this._socket.Listen(BACKLOG);

                this.AcceptAsync(this._socket);
            }
        }
        /// <summary>
        /// stop
        /// </summary>
        public void Stop()
        {
            if (this._socket != null)
            {
                this._socket.Close();
                this._socket = null;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// accept socket.
        /// </summary>
        /// <param name="socket"></param>
        private void AcceptAsync(Socket socket)
        {
            if (socket == null) return;

            bool completed = true;
            try { completed = this._socket.AcceptAsync(this._ae); }
            catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }

            if (!completed) ThreadPool.QueueUserWorkItem(_ => this.AcceptCompleted(this, this._ae));
        }
        /// <summary>
        /// async accept socket completed handle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            Socket accepted = null;
            if (e.SocketError == SocketError.Success) accepted = e.AcceptSocket;
            e.AcceptSocket = null;

            if (accepted != null)
                this.Accepted(this, this._host.NewConnection(accepted));

            //continue to accept!
            this.AcceptAsync(this._socket);
        }
        #endregion
    }
}