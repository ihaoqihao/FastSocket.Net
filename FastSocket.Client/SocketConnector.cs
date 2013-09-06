using System;
using System.Net;
using System.Net.Sockets;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// socket connector
    /// </summary>
    public sealed class SocketConnector
    {
        #region Members
        /// <summary>
        /// get node name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// get node endpoint
        /// </summary>
        private readonly EndPoint EndPoint;
        /// <summary>
        /// get node owner host
        /// </summary>
        private readonly SocketBase.IHost Host = null;

        private Action<SocketConnector, SocketBase.IConnection> _onConnected;
        private Action<SocketConnector, SocketBase.IConnection> _onDisconnected;
        private volatile bool _isStop = false;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endPoint"></param>
        /// <param name="host"></param>
        /// <param name="onConnected"></param>
        /// <param name="onDisconnected"></param>
        public SocketConnector(string name,
            EndPoint endPoint,
            SocketBase.IHost host,
            Action<SocketConnector, SocketBase.IConnection> onConnected,
            Action<SocketConnector, SocketBase.IConnection> onDisconnected)
        {
            this.Name = name;
            this.EndPoint = endPoint;
            this.Host = host;
            this._onConnected = onConnected;
            this._onDisconnected = onDisconnected;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// start
        /// </summary>
        public void Start()
        {
            BeginConnect(this.EndPoint, this.Host, connection =>
            {
                if (this._isStop)
                {
                    if (connection != null) connection.BeginDisconnect(); return;
                }
                if (connection == null)
                {
                    SocketBase.Utils.TaskEx.Delay(new Random().Next(1500, 3000), this.Start); return;
                }
                connection.Disconnected += this.OnDisconnected;
                this._onConnected(this, connection);
            });
        }
        /// <summary>
        /// stop
        /// </summary>
        public void Stop()
        {
            this._isStop = true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        private void OnDisconnected(SocketBase.IConnection connection, Exception ex)
        {
            connection.Disconnected -= this.OnDisconnected;
            //delay reconnect 20ms ~ 200ms
            if (!this._isStop) SocketBase.Utils.TaskEx.Delay(new Random().Next(20, 200), this.Start);
            //fire disconnected event
            this._onDisconnected(this, connection);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// begin connect
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="host"></param>
        /// <param name="callback"></param>
        /// <exception cref="ArgumentNullException">endPoint is null</exception>
        /// <exception cref="ArgumentNullException">host is null</exception>
        /// <exception cref="ArgumentNullException">callback is null</exception>
        static public void BeginConnect(EndPoint endPoint, SocketBase.IHost host, Action<SocketBase.IConnection> callback)
        {
            if (endPoint == null) throw new ArgumentNullException("endPoint");
            if (host == null) throw new ArgumentNullException("host");
            if (callback == null) throw new ArgumentNullException("callback");

            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.BeginConnect(endPoint, ar =>
                {
                    try
                    {
                        socket.EndConnect(ar);
                        socket.NoDelay = true;
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                        socket.ReceiveBufferSize = host.SocketBufferSize;
                        socket.SendBufferSize = host.SocketBufferSize;
                    }
                    catch (Exception ex)
                    {
                        try { socket.Close(); socket.Dispose(); }
                        catch { }

                        System.Diagnostics.Trace.TraceError(ex.ToString());
                        callback(null); return;
                    }

                    callback(new SocketBase.DefaultConnection(host.NextConnectionID(), socket, host));
                }, null);
            }
            catch { callback(null); }
        }
        #endregion
    }
}