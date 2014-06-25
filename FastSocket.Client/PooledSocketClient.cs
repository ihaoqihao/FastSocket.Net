using Sodao.FastSocket.SocketBase;
using System;
using System.Net;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// pooled socket client
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public class PooledSocketClient<TResponse> : BaseSocketClient<TResponse> where TResponse : class, Response.IResponse
    {
        #region Private Members
        private readonly IServerPool _serverPool = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="protocol"></param>
        public PooledSocketClient(Protocol.IProtocol<TResponse> protocol)
            : this(protocol, 8192, 9192, 3000, 3000)
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="socketBufferSize"></param>
        /// <param name="messageBufferSize"></param>
        /// <param name="millisecondsSendTimeout"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <exception cref="ArgumentNullException">protocol is null</exception>
        public PooledSocketClient(Protocol.IProtocol<TResponse> protocol,
            int socketBufferSize,
            int messageBufferSize,
            int millisecondsSendTimeout,
            int millisecondsReceiveTimeout)
            : base(protocol, socketBufferSize, messageBufferSize, millisecondsSendTimeout, millisecondsReceiveTimeout)
        {
            this._serverPool = this.InitServerPool();
            this._serverPool.Connected += this.OnServerPoolConnected;
            this._serverPool.ServerAvailable += this.OnServerPoolServerAvailable;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// create <see cref="IServerPool"/> instance.
        /// </summary>
        /// <returns></returns>
        protected virtual IServerPool InitServerPool()
        {
            return new DefaultServerPool(this);
        }
        /// <summary>
        /// on server pool connected
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connection"></param>
        protected virtual void OnServerPoolConnected(string name, IConnection connection)
        {
            base.RegisterConnection(connection);
        }
        /// <summary>
        /// on server available
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connection"></param>
        protected virtual void OnServerPoolServerAvailable(string name, IConnection connection)
        {
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="request"></param>
        protected override void Send(Request<TResponse> request)
        {
            var connection = this._serverPool.Acquire(request.ConsistentKey);
            if (connection == null) this.EnqueueToPendingQueue(request);//没有连接可用，放入待发送队列
            else connection.BeginSend(request);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// add server node
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool RegisterServerNode(string name, IPEndPoint endPoint)
        {
            return this._serverPool.TryRegisterNode(name, endPoint);
        }
        /// <summary>
        /// remove server node by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UnRegisterServerNode(string name)
        {
            return this._serverPool.UnRegisterNode(name);
        }
        /// <summary>
        /// get all node names
        /// </summary>
        /// <returns></returns>
        public string[] GetAllNodeNames()
        {
            return this._serverPool.GetAllNodeNames();
        }
        #endregion
    }
}