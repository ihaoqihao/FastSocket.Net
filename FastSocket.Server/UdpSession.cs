using System;
using System.Net;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// upd session
    /// </summary>
    public sealed class UdpSession
    {
        #region Members
        private readonly IUdpServer _server = null;
        /// <summary>
        /// get remote endPoint
        /// </summary>
        public readonly EndPoint RemoteEndPoint = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="server"></param>
        /// <exception cref="ArgumentNullException">server is null</exception>
        public UdpSession(EndPoint remoteEndPoint, IUdpServer server)
        {
            if (server == null) throw new ArgumentNullException("server");
            this.RemoteEndPoint = remoteEndPoint;
            this._server = server;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// sned async
        /// </summary>
        /// <param name="payload"></param>
        /// <exception cref="ArgumentNullException">payload is null or empty</exception>
        public void SendAsync(byte[] payload)
        {
            this._server.SendTo(this.RemoteEndPoint, payload);
        }
        #endregion
    }
}