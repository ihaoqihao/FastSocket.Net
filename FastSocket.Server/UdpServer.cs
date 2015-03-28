using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// upd server
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public sealed class UdpServer<TMessage> : IUdpServer<TMessage> where TMessage : class, Messaging.IMessage
    {
        #region Private Members
        private readonly int _port;
        private readonly int _messageBufferSize;

        private Socket _socket = null;
        private AsyncSendPool _pool = null;

        private readonly Protocol.IUdpProtocol<TMessage> _protocol = null;
        private readonly IUdpService<TMessage> _service = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="service"></param>
        public UdpServer(int port, Protocol.IUdpProtocol<TMessage> protocol,
            IUdpService<TMessage> service)
            : this(port, 2048, protocol, service)
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="protocol"></param>
        /// <param name="service"></param>
        /// <exception cref="ArgumentNullException">protocol is null.</exception>
        /// <exception cref="ArgumentNullException">service is null.</exception>
        public UdpServer(int port, int messageBufferSize,
            Protocol.IUdpProtocol<TMessage> protocol,
            IUdpService<TMessage> service)
        {
            if (protocol == null) throw new ArgumentNullException("protocol");
            if (service == null) throw new ArgumentNullException("service");

            this._port = port;
            this._messageBufferSize = messageBufferSize;
            this._protocol = protocol;
            this._service = service;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 异步接收数据
        /// </summary>
        /// <param name="e"></param>
        private void BeginReceive(SocketAsyncEventArgs e)
        {
            if (!this._socket.ReceiveFromAsync(e))
                ThreadPool.QueueUserWorkItem(_ => this.ReceiveCompleted(this, e));
        }
        /// <summary>
        /// completed handle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                var session = new UdpSession(e.RemoteEndPoint, this);
                TMessage message = null;
                try { message = this._protocol.Parse(new ArraySegment<byte>(e.Buffer, 0, e.BytesTransferred)); }
                catch (Exception ex)
                {
                    SocketBase.Log.Trace.Error(ex.Message, ex);
                    this._service.OnError(session, ex);
                }

                if (message != null) this._service.OnReceived(session, message);
            }

            //receive again
            this.BeginReceive(e);
        }
        #endregion

        #region IUdpServer Members
        /// <summary>
        /// start
        /// </summary>
        public void Start()
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this._socket.Bind(new IPEndPoint(IPAddress.Any, this._port));
            this._socket.DontFragment = true;

            this._pool = new AsyncSendPool(this._messageBufferSize, this._socket);

            var e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
            e.Completed += this.ReceiveCompleted;
            this.BeginReceive(e);
        }
        /// <summary>
        /// stop
        /// </summary>
        public void Stop()
        {
            this._socket.Close();
            this._socket = null;
            this._pool = null;
        }
        /// <summary>
        /// send to...
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="payload"></param>
        public void SendTo(EndPoint endPoint, byte[] payload)
        {
            this._pool.SendAsync(endPoint, payload);
        }
        #endregion

        /// <summary>
        /// 用于异步发送的<see cref="SocketAsyncEventArgs"/>对象池
        /// </summary>
        private class AsyncSendPool
        {
            #region Private Members
            private const int MAXPOOLSIZE = 3000;
            private readonly int _messageBufferSize;
            private readonly Socket _socket = null;
            private readonly ConcurrentStack<SocketAsyncEventArgs> _stack =
                new ConcurrentStack<SocketAsyncEventArgs>();
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="messageBufferSize"></param>
            /// <param name="socket"></param>
            public AsyncSendPool(int messageBufferSize, Socket socket)
            {
                if (socket == null) throw new ArgumentNullException("socket");
                this._messageBufferSize = messageBufferSize;
                this._socket = socket;
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// send completed handle
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void SendCompleted(object sender, SocketAsyncEventArgs e)
            {
                this.Release(e);
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// acquire
            /// </summary>
            /// <returns></returns>
            public SocketAsyncEventArgs Acquire()
            {
                SocketAsyncEventArgs e;
                if (this._stack.TryPop(out e)) return e;

                e = new SocketAsyncEventArgs();
                e.SetBuffer(new byte[this._messageBufferSize], 0, this._messageBufferSize);
                e.Completed += this.SendCompleted;
                return e;
            }
            /// <summary>
            /// release
            /// </summary>
            /// <param name="e"></param>
            public void Release(SocketAsyncEventArgs e)
            {
                if (this._stack.Count >= MAXPOOLSIZE)
                {
                    e.Completed -= this.SendCompleted;
                    e.Dispose();
                    return;
                }

                this._stack.Push(e);
            }
            /// <summary>
            /// sned async
            /// </summary>
            /// <param name="endPoint"></param>
            /// <param name="payload"></param>
            /// <exception cref="ArgumentNullException">endPoint is null</exception>
            /// <exception cref="ArgumentNullException">payload is null or empty</exception>
            /// <exception cref="ArgumentOutOfRangeException">payload length大于messageBufferSize</exception>
            public void SendAsync(EndPoint endPoint, byte[] payload)
            {
                if (endPoint == null) throw new ArgumentNullException("endPoint");
                if (payload == null || payload.Length == 0) throw new ArgumentNullException("payload");
                if (payload.Length > this._messageBufferSize)
                    throw new ArgumentOutOfRangeException("payload.Length", "payload length大于messageBufferSize");

                var e = this.Acquire();
                e.RemoteEndPoint = endPoint;

                Buffer.BlockCopy(payload, 0, e.Buffer, 0, payload.Length);
                e.SetBuffer(0, payload.Length);

                if (!this._socket.SendToAsync(e))
                    ThreadPool.QueueUserWorkItem(_ => this.Release(e));
            }
            #endregion
        }
    }
}