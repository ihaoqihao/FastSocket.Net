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
        /// <summary>
        /// connection collection
        /// </summary>
        protected readonly ConnectionCollection _listConnections = new ConnectionCollection();
        private readonly ConcurrentStack<SocketAsyncEventArgs> _stack = new ConcurrentStack<SocketAsyncEventArgs>();
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
            if (!connection.Active) return;

            connection.StartSending += new StartSendingHandler(this.OnStartSending);
            connection.SendCallback += new SendCallbackHandler(this.OnSendCallback);
            connection.MessageReceived += new MessageReceivedHandler(this.OnMessageReceived);
            connection.Disconnected += new DisconnectedHandler(this.OnDisconnected);
            connection.Error += new ErrorHandler(this.OnError);

            this._listConnections.Add(connection);
            this.OnConnected(connection);
        }
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected virtual void OnConnected(IConnection connection)
        {
            Log.Logger.Debug(string.Concat("socket connected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? string.Empty : connection.RemoteEndPoint.ToString()));
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
        /// <param name="e"></param>
        protected virtual void OnSendCallback(IConnection connection, SendCallbackEventArgs e)
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
        protected virtual void OnDisconnected(IConnection connection, Exception ex)
        {
            this._listConnections.Remove(connection.ConnectionID);

            connection.StartSending -= new StartSendingHandler(this.OnStartSending);
            connection.SendCallback -= new SendCallbackHandler(this.OnSendCallback);
            connection.MessageReceived -= new MessageReceivedHandler(this.OnMessageReceived);
            connection.Disconnected -= new DisconnectedHandler(this.OnDisconnected);
            connection.Error -= new ErrorHandler(this.OnError);

            Log.Logger.Debug(string.Concat("socket disconnected, id:", connection.ConnectionID.ToString(),
                ", remot endPoint:", connection.RemoteEndPoint == null ? string.Empty : connection.RemoteEndPoint.ToString(),
                ex == null ? string.Empty : string.Concat(", reason is: ", ex.ToString())));
        }
        /// <summary>
        /// OnError
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected virtual void OnError(IConnection connection, Exception ex)
        {
            Log.Logger.Error(ex.Message, ex);
        }
        #endregion

        #region ISAEAPool Members
        /// <summary>
        /// get
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs GetSocketAsyncEventArgs()
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
        public void ReleaseSocketAsyncEventArgs(SocketAsyncEventArgs e)
        {
            if (e.Buffer == null || e.Buffer.Length != this.MessageBufferSize)
            {
                e.Dispose(); return;
            }

            if (this._stack.Count >= 50000) { e.Dispose(); return; }

            this._stack.Push(e);
        }
        #endregion
    }
}