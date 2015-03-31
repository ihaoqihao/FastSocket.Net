using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// endPoint manager interface
    /// </summary>
    public interface IEndPointManager<TMessage> where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// init
        /// </summary>
        /// <param name="client"></param>
        /// <param name="connectionPool"></param>
        void Init(SocketClient<TMessage> client,
            IConnectionPool<TMessage> connectionPool);
        /// <summary>
        /// try register
        /// </summary>
        /// <param name="name"></param>
        /// <param name="remoteEP"></param>
        /// <param name="initFunc"></param>
        /// <returns></returns>
        bool TryRegister(string name, EndPoint remoteEP,
            Func<SendContext<TMessage>, Task> initFunc);
        /// <summary>
        /// un register
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool UnRegister(string name);
        /// <summary>
        /// to array
        /// </summary>
        /// <returns></returns>
        KeyValuePair<string, EndPoint>[] ToArray();
        /// <summary>
        /// try acquire
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        bool TryAcquire(out SocketBase.IConnection connection);
    }
}