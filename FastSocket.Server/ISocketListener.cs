using System;
using System.Net;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// socket listener
    /// </summary>
    public interface ISocketListener
    {
        /// <summary>
        /// socket accepted event
        /// </summary>
        event Action<ISocketListener, SocketBase.IConnection> Accepted;

        /// <summary>
        /// get endpoint
        /// </summary>
        EndPoint EndPoint { get; }
        /// <summary>
        /// start listen
        /// </summary>
        void Start();
        /// <summary>
        /// stop listen
        /// </summary>
        void Stop();
    }
}