using System;
using System.Net;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// socket server.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class SocketServer<TMessage> : SocketBase.BaseHost where TMessage : class, Messaging.IMessage
    {
        #region Private Members
        private readonly SocketListener _listener = null;
        private readonly ISocketService<TMessage> _socketService = null;
        private readonly Protocol.IProtocol<TMessage> _protocol = null;
        private readonly int _maxMessageSize;
        private readonly int _maxConnections;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="port"></param>
        /// <param name="socketService"></param>
        /// <param name="protocol"></param>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="maxConnections"></param>
        /// <exception cref="ArgumentNullException">socketService is null.</exception>
        /// <exception cref="ArgumentNullException">protocol is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxMessageSize</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxConnections</exception>
        public SocketServer(int port,
            ISocketService<TMessage> socketService,
            Protocol.IProtocol<TMessage> protocol,
            int socketBufferSize,
            int messageBufferSize,
            int maxMessageSize,
            int maxConnections)
            : base(socketBufferSize, messageBufferSize)
        {
            if (socketService == null) throw new ArgumentNullException("socketService");
            if (protocol == null) throw new ArgumentNullException("protocol");
            if (maxMessageSize < 1) throw new ArgumentOutOfRangeException("maxMessageSize");
            if (maxConnections < 1) throw new ArgumentOutOfRangeException("maxConnections");

            this._socketService = socketService;
            this._protocol = protocol;
            this._maxMessageSize = maxMessageSize;
            this._maxConnections = maxConnections;

            this._listener = new SocketListener(new IPEndPoint(IPAddress.Any, port), this);
            this._listener.Accepted += this.OnAccepted;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// socket accepted handler
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="connection"></param>
        private void OnAccepted(ISocketListener listener, SocketBase.IConnection connection)
        {
            if (base.CountConnection() < this._maxConnections)
            {
                base.RegisterConnection(connection);
                return;
            }

            SocketBase.Log.Trace.Info("too many connections.");
            connection.BeginDisconnect();
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// start
        /// </summary>
        public override void Start()
        {
            base.Start();
            this._listener.Start();
        }
        /// <summary>
        /// stop
        /// </summary>
        public override void Stop()
        {
            this._listener.Stop();
            base.Stop();
        }
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnConnected(SocketBase.IConnection connection)
        {
            base.OnConnected(connection);
            this._socketService.OnConnected(connection);
        }
        /// <summary>
        /// send callback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="isSuccess"></param>
        protected override void OnSendCallback(SocketBase.IConnection connection,
            SocketBase.Packet packet, bool isSuccess)
        {
            base.OnSendCallback(connection, packet, isSuccess);
            this._socketService.OnSendCallback(connection, packet, isSuccess);
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnMessageReceived(SocketBase.IConnection connection,
            SocketBase.MessageReceivedEventArgs e)
        {
            base.OnMessageReceived(connection, e);

            int readlength;
            TMessage message = null;
            try { message = this._protocol.Parse(connection, e.Buffer, this._maxMessageSize, out readlength); }
            catch (Exception ex)
            {
                this.OnConnectionError(connection, ex);
                connection.BeginDisconnect(ex);
                e.SetReadlength(e.Buffer.Count);
                return;
            }

            if (message != null) this._socketService.OnReceived(connection, message);
            e.SetReadlength(readlength);
        }
        /// <summary>
        /// OnDisconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected override void OnDisconnected(SocketBase.IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);
            this._socketService.OnDisconnected(connection, ex);
        }
        /// <summary>
        /// on connection error
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected override void OnConnectionError(SocketBase.IConnection connection, Exception ex)
        {
            base.OnConnectionError(connection, ex);
            this._socketService.OnException(connection, ex);
        }
        #endregion
    }
}