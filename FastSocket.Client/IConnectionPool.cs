using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// connection pool interface
    /// </summary>
    public interface IConnectionPool<TMessage>
        where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// init
        /// </summary>
        /// <param name="client"></param>
        void Init(SocketClient<TMessage> client);
        /// <summary>
        /// try acquire <see cref="IConnection"/>
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        bool TryAcquire(out SocketBase.IConnection connection);
    }
}