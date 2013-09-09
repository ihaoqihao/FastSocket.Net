using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Sodao.FastSocket.SocketBase.Utils;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// default server pool
    /// </summary>
    public sealed class DefaultServerPool : IServerPool
    {
        #region Private Members
        private SocketBase.IHost _host = null;
        private int _acquireTimes = 0;

        /// <summary>
        /// key:node name
        /// </summary>
        private readonly Dictionary<string, SocketConnector> _dicNodes =
            new Dictionary<string, SocketConnector>();
        /// <summary>
        /// key:node name
        /// value:socket connection
        /// </summary>
        private readonly Dictionary<string, SocketBase.IConnection> _dicConnections =
            new Dictionary<string, SocketBase.IConnection>();
        /// <summary>
        /// socket connection array.
        /// </summary>
        private SocketBase.IConnection[] _arrConnections = null;
        /// <summary>
        /// consistent hash connections.
        /// </summary>
        private ConsistentHashContainer<SocketBase.IConnection> _hashConnections = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="host"></param>
        /// <exception cref="ArgumentNullException">host is null</exception>
        public DefaultServerPool(SocketBase.IHost host)
        {
            if (host == null) throw new ArgumentNullException("host");
            this._host = host;
        }
        #endregion

        #region IServerPool Members
        /// <summary>
        /// socket connected event
        /// </summary>
        public event Action<string, SocketBase.IConnection> Connected;
        /// <summary>
        /// server available event
        /// </summary>
        public event Action<string, SocketBase.IConnection> ServerAvailable;

        /// <summary>
        /// try register server node.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool TryRegisterNode(string name, EndPoint endPoint)
        {
            SocketConnector node = null;
            lock (this)
            {
                if (this._dicNodes.ContainsKey(name)) return false;
                this._dicNodes[name] = node = new SocketConnector(name, endPoint, this._host,
                    this.OnConnected, this.OnDisconnected);
            }
            node.Start();
            return true;
        }
        /// <summary>
        /// remove server node
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">name is null or empty</exception>
        public bool UnRegisterNode(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            SocketConnector node = null;
            SocketBase.IConnection connection = null;
            lock (this)
            {
                //remove node by name,
                if (this._dicNodes.TryGetValue(name, out node)) this._dicNodes.Remove(name);
                //get connection by name.
                this._dicConnections.TryGetValue(name, out connection);
            }

            if (node != null) node.Stop();
            if (connection != null) connection.BeginDisconnect();
            return node != null;
        }
        /// <summary>
        /// acquire a connection
        /// </summary>
        /// <returns></returns>
        public SocketBase.IConnection Acquire()
        {
            var arr = this._arrConnections;
            if (arr == null || arr.Length == 0) return null;

            if (arr.Length == 1) return arr[0];
            return arr[(Interlocked.Increment(ref this._acquireTimes) & 0x7fffffff) % arr.Length];
        }
        /// <summary>
        /// acquire a connection
        /// </summary>
        /// <param name="hash">一致性哈希值</param>
        /// <returns></returns>
        public SocketBase.IConnection Acquire(byte[] hash)
        {
            if (hash == null || hash.Length == 0) return this.Acquire();

            var hashConnections = this._hashConnections;
            if (hashConnections == null) return null;
            return hashConnections.Get(hash);
        }
        /// <summary>
        /// get all node names
        /// </summary>
        /// <returns></returns>
        public string[] GetAllNodeNames()
        {
            lock (this) return this._dicNodes.Keys.ToArray();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// on connected
        /// </summary>
        /// <param name="node"></param>
        /// <param name="connection"></param>
        private void OnConnected(SocketConnector node, SocketBase.IConnection connection)
        {
            //fire connected event.
            this.Connected(node.Name, connection);

            bool isActive = false;
            SocketBase.IConnection oldConnection = null;
            lock (this)
            {
                //remove exists connection by name.
                if (this._dicConnections.TryGetValue(node.Name, out oldConnection)) this._dicConnections.Remove(node.Name);
                //add curr connection to list if node is active
                if (isActive = this._dicNodes.ContainsKey(node.Name)) this._dicConnections[node.Name] = connection;

                this._arrConnections = this._dicConnections.Values.ToArray();
                this._hashConnections = new ConsistentHashContainer<SocketBase.IConnection>(this._dicConnections);
            }
            //disconect old connection.
            if (oldConnection != null) oldConnection.BeginDisconnect();
            //disconnect not active node connection.
            if (!isActive) connection.BeginDisconnect();
            //fire server available event.
            if (isActive && this.ServerAvailable != null) this.ServerAvailable(node.Name, connection);
        }
        /// <summary>
        /// on disconnected
        /// </summary>
        /// <param name="node"></param>
        /// <param name="connection"></param>
        private void OnDisconnected(SocketConnector node, SocketBase.IConnection connection)
        {
            lock (this)
            {
                if (!this._dicConnections.Remove(node.Name)) return;

                this._arrConnections = this._dicConnections.Values.ToArray();
                this._hashConnections = new ConsistentHashContainer<SocketBase.IConnection>(this._dicConnections);
            }
        }
        #endregion
    }
}