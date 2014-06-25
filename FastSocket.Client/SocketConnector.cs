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
        #region Events
        /// <summary>
        /// connected
        /// </summary>
        public event Action<SocketConnector, SocketBase.IConnection> Connected;
        /// <summary>
        /// disconnected
        /// </summary>
        public event Action<SocketConnector, SocketBase.IConnection> Disconnected;
        #endregion

        #region Members
        /// <summary>
        /// get node name
        /// </summary>
        public readonly string Name;

        private readonly EndPoint EndPoint;
        private readonly SocketBase.IHost Host = null;
        private volatile bool _isStop = false;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="name"></param>
        /// <param name="endPoint"></param>
        /// <param name="host"></param>
        public SocketConnector(string name, EndPoint endPoint, SocketBase.IHost host)
        {
            this.Name = name;
            this.EndPoint = endPoint;
            this.Host = host;
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
                    if (connection != null) connection.BeginDisconnect();
                    return;
                }
                if (connection == null)
                {
                    SocketBase.Utils.TaskEx.Delay(new Random().Next(1500, 3000), this.Start);
                    return;
                }
                connection.Disconnected += this.OnDisconnected;
                this.Connected(this, connection);
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
            //fire disconnected event
            this.Disconnected(this, connection);
            //delay reconnect 20ms ~ 200ms
            if (!this._isStop) SocketBase.Utils.TaskEx.Delay(new Random().Next(20, 200), this.Start);
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

            SocketBase.Log.Trace.Debug(string.Concat("begin connect to ", endPoint.ToString()));

            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.BeginConnect(endPoint, result =>
                {
                    try
                    {
                        socket.EndConnect(result);
                        socket.NoDelay = true;
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                        socket.ReceiveBufferSize = host.SocketBufferSize;
                        socket.SendBufferSize = host.SocketBufferSize;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            socket.Close();
                            socket.Dispose();
                        }
                        catch { }

                        SocketBase.Log.Trace.Error(ex.Message, ex);
                        callback(null); return;
                    }
                    callback(host.NewConnection(socket));
                }, null);
            }
            catch (Exception ex)
            {
                SocketBase.Log.Trace.Error(ex.Message, ex);
                callback(null);
            }
        }
        #endregion
    }
}