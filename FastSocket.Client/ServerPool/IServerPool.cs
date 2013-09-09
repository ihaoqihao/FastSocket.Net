using System;
using System.Net;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// server pool interface.
    /// </summary>
    public interface IServerPool
    {
        /// <summary>
        /// socket connected event
        /// </summary>
        event Action<string, SocketBase.IConnection> Connected;
        /// <summary>
        /// server available event
        /// </summary>
        event Action<string, SocketBase.IConnection> ServerAvailable;

        /// <summary>
        /// try register server node.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        bool TryRegisterNode(string name, EndPoint endPoint);
        /// <summary>
        /// remove server node
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool UnRegisterNode(string name);
        /// <summary>
        /// acquire a connection
        /// </summary>
        /// <returns></returns>
        SocketBase.IConnection Acquire();
        /// <summary>
        /// acquire a connection
        /// </summary>
        /// <param name="hash">一致性哈希值</param>
        /// <returns></returns>
        SocketBase.IConnection Acquire(byte[] hash);
        /// <summary>
        /// get all node names
        /// </summary>
        /// <returns></returns>
        string[] GetAllNodeNames();
    }
}