using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// base host
    /// </summary>
    public abstract class BaseHost : IHost
    {
        #region Members
        private long _connectionID = 1000L;
        private readonly ConnectionCollection _listConnections = new ConnectionCollection();
        private readonly ConcurrentStack<SocketAsyncEventArgs> _stack = new ConcurrentStack<SocketAsyncEventArgs>();
        private readonly Adapter _adapter = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <exception cref="ArgumentOutOfRangeException">socketBufferSize</exception>
        /// <exception cref="ArgumentOutOfRangeException">messageBufferSize</exception>
        protected BaseHost(int socketBufferSize, int messageBufferSize)
        {
            if (socketBufferSize < 1) throw new ArgumentOutOfRangeException("socketBufferSize");
            if (messageBufferSize < 1) throw new ArgumentOutOfRangeException("messageBufferSize");

            this.SocketBufferSize = socketBufferSize;
            this.MessageBufferSize = messageBufferSize;

            this._adapter = new Adapter(this.GetSocketAsyncEventArgs,
                this.ReleaseSocketAsyncEventArgs,
                this.OnStartSending,
                this.OnSendCallback,
                this.OnMessageReceived,
                this.OnDisconnected,
                this.OnConnectionError);
        }
        #endregion

        #region IHost Members
        /// <summary>
        /// get socket buffer size
        /// </summary>
        public int SocketBufferSize
        {
            get;
            private set;
        }
        /// <summary>
        /// get message buffer size
        /// </summary>
        public int MessageBufferSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 生成下一个连接ID
        /// </summary>
        /// <returns></returns>
        public long NextConnectionID()
        {
            return Interlocked.Increment(ref this._connectionID);
        }
        /// <summary>
        /// create new <see cref="IConnection"/>
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">socket is null</exception>
        public virtual IConnection NewConnection(Socket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            return new DefaultConnection(this.NextConnectionID(), socket, this, this._adapter);
        }
        /// <summary>
        /// get <see cref="IConnection"/> by connectionID
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        public IConnection GetConnectionByID(long connectionID)
        {
            return this._listConnections.Get(connectionID);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public virtual void Start()
        {
        }
        /// <summary>
        /// 停止
        /// </summary>
        public virtual void Stop()
        {
            this._listConnections.DisconnectAll();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// register connection
        /// </summary>
        /// <param name="connection"></param>
        /// <exception cref="ArgumentNullException">connection is null</exception>
        protected void RegisterConnection(IConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (connection.Active)
            {
                this._listConnections.Add(connection);
                this.OnConnected(connection);
            }
        }
        /// <summary>
        /// get connection count.
        /// </summary>
        /// <returns></returns>
        protected int CountConnection()
        {
            return this._listConnections.Count();
        }

        /// <summary>
        /// get
        /// </summary>
        /// <returns></returns>
        protected SocketAsyncEventArgs GetSocketAsyncEventArgs()
        {
            SocketAsyncEventArgs e;
            if (this._stack.TryPop(out e)) return e;

            e = new SocketAsyncEventArgs();
            e.SetBuffer(new byte[this.MessageBufferSize], 0, this.MessageBufferSize);
            return e;
        }
        /// <summary>
        /// release
        /// </summary>
        /// <param name="e"></param>
        protected void ReleaseSocketAsyncEventArgs(SocketAsyncEventArgs e)
        {
            if (e.Buffer == null || e.Buffer.Length != this.MessageBufferSize) { e.Dispose(); return; }
            if (this._stack.Count >= 50000) { e.Dispose(); return; }
            this._stack.Push(e);
        }

        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected virtual void OnConnected(IConnection connection)
        {
            Log.Trace.Debug(string.Concat("socket connected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? string.Empty : connection.RemoteEndPoint.ToString(),
                ", local endPoint:", connection.LocalEndPoint == null ? string.Empty : connection.LocalEndPoint.ToString()));
        }
        /// <summary>
        /// OnStartSending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        protected virtual void OnStartSending(IConnection connection, Packet packet)
        {
        }
        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="status"></param>
        protected virtual void OnSendCallback(IConnection connection, Packet packet, SendStatus status)
        {
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected virtual void OnMessageReceived(IConnection connection, MessageReceivedEventArgs e)
        {
        }
        /// <summary>
        /// OnDisconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        /// <exception cref="ArgumentNullException">connection is null</exception>
        protected virtual void OnDisconnected(IConnection connection, Exception ex)
        {
            this._listConnections.Remove(connection.ConnectionID);

            Log.Trace.Debug(string.Concat("socket disconnected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? string.Empty : connection.RemoteEndPoint.ToString(),
                ", local endPoint:", connection.LocalEndPoint == null ? string.Empty : connection.LocalEndPoint.ToString(),
                ex == null ? string.Empty : string.Concat(", reason is: ", ex.ToString())));
        }
        /// <summary>
        /// OnError
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected virtual void OnConnectionError(IConnection connection, Exception ex)
        {
            Log.Trace.Error(ex.Message, ex);
        }
        #endregion

        /// <summary>
        /// adapter
        /// </summary>
        internal sealed class Adapter
        {
            #region Public Members
            /// <summary>
            /// get <see cref="SocketAsyncEventArgs"/>
            /// </summary>
            public readonly Func<SocketAsyncEventArgs> GetSocketAsyncEventArgs;
            /// <summary>
            /// release <see cref="SocketAsyncEventArgs"/>
            /// </summary>
            /// <returns></returns>
            public readonly Action<SocketAsyncEventArgs> ReleaseSocketAsyncEventArgs;

            /// <summary>
            /// on packet start sending
            /// </summary>
            public readonly Action<IConnection, Packet> OnStartSending;
            /// <summary>
            /// packet send callback event
            /// </summary>
            public readonly Action<IConnection, Packet, SendStatus> OnSendCallback;
            /// <summary>
            /// on message received
            /// </summary>
            public readonly Action<IConnection, MessageReceivedEventArgs> OnMessageReceived;
            /// <summary>
            /// on connection disconnected
            /// </summary>
            public readonly Action<IConnection, Exception> OnDisconnected;
            /// <summary>
            /// on connection error event
            /// </summary>
            public readonly Action<IConnection, Exception> OnConnectionError;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="funGetSocketAsyncEventArgs"></param>
            /// <param name="releaseSocketAsyncEventArgs"></param>
            /// <param name="onStartSending"></param>
            /// <param name="onSendCallback"></param>
            /// <param name="onMessageReceived"></param>
            /// <param name="onDisconnected"></param>
            /// <param name="onConnectionError"></param>
            public Adapter(
                Func<SocketAsyncEventArgs> funGetSocketAsyncEventArgs,
                Action<SocketAsyncEventArgs> releaseSocketAsyncEventArgs,
                Action<IConnection, Packet> onStartSending,
                Action<IConnection, Packet, SendStatus> onSendCallback,
                Action<IConnection, MessageReceivedEventArgs> onMessageReceived,
                Action<IConnection, Exception> onDisconnected,
                Action<IConnection, Exception> onConnectionError)
            {
                this.GetSocketAsyncEventArgs = funGetSocketAsyncEventArgs;
                this.ReleaseSocketAsyncEventArgs = releaseSocketAsyncEventArgs;

                this.OnStartSending = onStartSending;
                this.OnSendCallback = onSendCallback;
                this.OnMessageReceived = onMessageReceived;
                this.OnDisconnected = onDisconnected;
                this.OnConnectionError = onConnectionError;
            }
            #endregion
        }
    }
}