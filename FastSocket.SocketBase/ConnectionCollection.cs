using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Sodao.FastSocket.SocketBase
{
    /// <summary>
    /// socket connection collection
    /// </summary>
    public sealed class ConnectionCollection
    {
        #region Private Members
        /// <summary>
        /// key:ConnectionID
        /// </summary>
        private readonly ConcurrentDictionary<long, IConnection> _dic =
            new ConcurrentDictionary<long, IConnection>();
        #endregion

        #region Public Methods
        /// <summary>
        /// add
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">connection is null</exception>
        public bool Add(IConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            return this._dic.TryAdd(connection.ConnectionID, connection);
        }
        /// <summary>
        /// remove connection by id.
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        public bool Remove(long connectionID)
        {
            IConnection connection;
            return this._dic.TryRemove(connectionID, out connection);
        }
        /// <summary>
        /// get by connection id
        /// </summary>
        /// <param name="connectionID"></param>
        /// <returns></returns>
        public IConnection Get(long connectionID)
        {
            IConnection connection;
            this._dic.TryGetValue(connectionID, out connection);
            return connection;
        }
        /// <summary>
        /// to array
        /// </summary>
        /// <returns></returns>
        public IConnection[] ToArray()
        {
            return this._dic.ToArray().Select(c => c.Value).ToArray();
        }
        /// <summary>
        /// count.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return this._dic.Count;
        }
        /// <summary>
        /// 断开所有连接
        /// </summary>
        public void DisconnectAll()
        {
            var connections = this.ToArray();
            foreach (var conn in connections) conn.BeginDisconnect();
        }
        #endregion
    }
}