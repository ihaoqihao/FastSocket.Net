using Sodao.FastSocket.SocketBase;
using Sodao.FastSocket.SocketBase.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// socket client
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class SocketClient<TMessage> : SocketBase.BaseHost
        where TMessage : class, Messaging.IMessage
    {
        #region Event
        /// <summary>
        /// received unknow message
        /// </summary>
        public event Action<IConnection, TMessage> UnknowMessageReceived;
        #endregion

        #region Private Members
        private int _seqId = 0;
        private readonly Protocol.IProtocol<TMessage> _protocol = null;

        private readonly int _millisecondsSendTimeout;
        private readonly int _millisecondsReceiveTimeout;

        private readonly PendingSendQueue _pendingQueue = null;
        private readonly ReceivingQueue _receivingQueue = null;

        private readonly SocketConnector _connector = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="protocol"></param>
        public SocketClient(Protocol.IProtocol<TMessage> protocol)
            : this(protocol, 8192, 8192, 3000, 3000)
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
        public SocketClient(Protocol.IProtocol<TMessage> protocol,
            int socketBufferSize,
            int messageBufferSize,
            int millisecondsSendTimeout,
            int millisecondsReceiveTimeout)
            : base(socketBufferSize, messageBufferSize)
        {
            if (protocol == null) throw new ArgumentNullException("protocol");
            this._protocol = protocol;

            this._millisecondsSendTimeout = millisecondsSendTimeout;
            this._millisecondsReceiveTimeout = millisecondsReceiveTimeout;

            this._pendingQueue = new PendingSendQueue(this);
            this._receivingQueue = new ReceivingQueue();

            this._connector = new SocketConnector(this);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// 发送超时毫秒数
        /// </summary>
        public int MillisecondsSendTimeout
        {
            get { return this._millisecondsSendTimeout; }
        }
        /// <summary>
        /// 接收超时毫秒数
        /// </summary>
        public int MillisecondsReceiveTimeout
        {
            get { return this._millisecondsReceiveTimeout; }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 处理未知的message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        protected virtual void HandleUnknowMessage(IConnection connection, TMessage message)
        {
            if (this.UnknowMessageReceived != null)
                this.UnknowMessageReceived(connection, message);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnConnected(IConnection connection)
        {
            base.OnConnected(connection);
            connection.BeginReceive();//异步开始接收数据
        }
        /// <summary>
        /// OnStartSending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        protected override void OnStartSending(IConnection connection, Packet packet)
        {
            base.OnStartSending(connection, packet);
            this._receivingQueue.TryAdd(packet as Request<TMessage>);
        }
        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="isSuccess"></param>
        protected override void OnSendCallback(IConnection connection, Packet packet, bool isSuccess)
        {
            base.OnSendCallback(connection, packet, isSuccess);

            var r = packet as Request<TMessage>;
            if (isSuccess)
            {
                r.SentTime = DateTime.UtcNow; return;
            }

            this._receivingQueue.TryRemove(r.SeqId);
            if (DateTime.UtcNow.Subtract(r.CreatedTime).TotalMilliseconds < this._millisecondsSendTimeout)
            {
                this.Send(r); return;
            }

            //send time out
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { r.SetException(new RequestException(RequestException.Errors.PendingSendTimeout, r.Name)); }
                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnMessageReceived(IConnection connection, MessageReceivedEventArgs e)
        {
            base.OnMessageReceived(connection, e);

            int readlength;
            TMessage message = null;
            try { message = this._protocol.Parse(connection, e.Buffer, out readlength); }
            catch (Exception ex)
            {
                base.OnConnectionError(connection, ex);
                connection.BeginDisconnect(ex);
                e.SetReadlength(e.Buffer.Count);
                return;
            }

            if (message != null)
            {
                var r = this._receivingQueue.TryRemove(message.SeqId);
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        if (r == null) this.HandleUnknowMessage(connection, message);
                        else r.SetResult(message);
                    }
                    catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
                });
            }

            e.SetReadlength(readlength);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// try register remote server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="remoteEP"></param>
        /// <returns></returns>
        public bool TryRegisterRemoteServer(string name, EndPoint remoteEP)
        {
            return this._connector.TryRegister(name, remoteEP);
        }
        /// <summary>
        /// un register remote server
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UnRegisterRemoteServer(string name)
        {
            return this._connector.UnRegister(name);
        }
        /// <summary>
        /// remote server to array
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, EndPoint>[] ToArrayRegisteredServer()
        {
            return this._connector.ToArray();
        }
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="request"></param>
        public void Send(Request<TMessage> request)
        {
            IConnection connection = null;
            if (!this._connector.TryAcquire(out connection))
            {
                this._pendingQueue.Enqueue(request);
                return;
            }
            connection.BeginSend(request);
        }
        /// <summary>
        /// 产生不重复的seqID
        /// </summary>
        /// <returns></returns>
        public int NextRequestSeqID()
        {
            return Interlocked.Increment(ref this._seqId) & 0x7fffffff;
        }
        #endregion

        /// <summary>
        /// send queue
        /// </summary>
        private class PendingSendQueue
        {
            #region Private Members
            private readonly SocketClient<TMessage> _client = null;
            private readonly ConcurrentQueue<Request<TMessage>> _queue =
                new ConcurrentQueue<Request<TMessage>>();
            private readonly Timer _timer = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="client"></param>
            public PendingSendQueue(SocketClient<TMessage> client)
            {
                if (client == null) throw new ArgumentNullException("client");
                this._client = client;

                this._timer = new Timer(state =>
                {
                    var count = this._queue.Count;
                    if (count == 0) return;

                    this._timer.Change(Timeout.Infinite, Timeout.Infinite);

                    var dtNow = SocketBase.Utils.Date.UtcNow;
                    var timeOut = this._client.MillisecondsSendTimeout;
                    while (count-- > 0)
                    {
                        Request<TMessage> request;
                        if (!this._queue.TryDequeue(out request)) break;

                        //try send...
                        if (dtNow.Subtract(request.CreatedTime).TotalMilliseconds < timeOut)
                        {
                            this._client.Send(request);
                            continue;
                        }

                        //fire send time out
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try { request.SetException(new RequestException(RequestException.Errors.PendingSendTimeout, request.Name)); }
                            catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
                        });
                    }

                    this._timer.Change(100, 0);
                }, null, 100, 0);
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// 入列
            /// </summary>
            /// <param name="request"></param>
            /// <exception cref="ArgumentNullException">request is null</exception>
            public void Enqueue(Request<TMessage> request)
            {
                if (request == null) throw new ArgumentNullException("request");
                this._queue.Enqueue(request);
            }
            #endregion
        }

        /// <summary>
        /// receiving queue
        /// </summary>
        private class ReceivingQueue
        {
            #region Private Members
            private readonly ConcurrentDictionary<int, Request<TMessage>> _dic =
                new ConcurrentDictionary<int, Request<TMessage>>();
            private readonly Timer _timer = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            public ReceivingQueue()
            {
                this._timer = new Timer(state =>
                {
                    if (this._dic.Count == 0) return;
                    this._timer.Change(Timeout.Infinite, Timeout.Infinite);

                    var dtNow = SocketBase.Utils.Date.UtcNow;
                    var arr = this._dic.ToArray().Where(c =>
                        dtNow.Subtract(c.Value.SentTime).TotalMilliseconds > c.Value.MillisecondsReceiveTimeout).ToArray();

                    for (int i = 0, l = arr.Length; i < l; i++)
                    {
                        Request<TMessage> request;
                        if (this._dic.TryRemove(arr[i].Key, out request))
                        {
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                try { request.SetException(new RequestException(RequestException.Errors.ReceiveTimeout, request.Name)); }
                                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
                            });
                        }
                    }

                    this._timer.Change(500, 0);
                }, null, 500, 0);
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// try add
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public bool TryAdd(Request<TMessage> request)
            {
                return this._dic.TryAdd(request.SeqId, request);
            }
            /// <summary>
            /// remove
            /// </summary>
            /// <param name="seqID"></param>
            /// <returns></returns>
            public Request<TMessage> TryRemove(int seqID)
            {
                Request<TMessage> request = null;
                this._dic.TryRemove(seqID, out request);
                return request;
            }
            #endregion
        }

        /// <summary>
        /// socket connector
        /// </summary>
        private class SocketConnector
        {
            #region Members
            /// <summary>
            /// socket client
            /// </summary>
            private readonly SocketClient<TMessage> _client = null;
            /// <summary>
            /// key:name
            /// </summary>
            private readonly Dictionary<string, EndPoint> _dicRemote =
                new Dictionary<string, EndPoint>();
            /// <summary>
            /// key:node name
            /// </summary>
            private readonly Dictionary<string, IConnection> _dicConnections =
                new Dictionary<string, IConnection>();
            /// <summary>
            /// <see cref="IConnection"/> array.
            /// </summary>
            private IConnection[] _arrConnections = null;
            /// <summary>
            /// acquire <see cref="IConnection"/> number
            /// </summary>
            private int _acquireNumber = 0;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="client"></param>
            /// <exception cref="ArgumentNullException">host is null</exception>
            public SocketConnector(SocketClient<TMessage> client)
            {
                if (client == null) throw new ArgumentNullException("client");
                this._client = client;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// try register remote server
            /// </summary>
            /// <param name="name"></param>
            /// <param name="remoteEP"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">name is null or empty</exception>
            /// <exception cref="ArgumentNullException">remoteEP is null</exception>
            public bool TryRegister(string name, EndPoint remoteEP)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
                if (remoteEP == null) throw new ArgumentNullException("remoteEP");

                lock (this)
                {
                    if (this._dicRemote.ContainsKey(name)) return false;
                    this._dicRemote[name] = remoteEP;
                }

                this.Connect(name, remoteEP);
                return true;
            }
            /// <summary>
            /// un register remote server
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">name is null or empty</exception>
            public bool UnRegister(string name)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

                lock (this)
                {
                    if (!this._dicRemote.Remove(name)) return false;

                    IConnection connection;
                    if (!this._dicConnections.TryGetValue(name, out connection)) return true;
                    connection.BeginDisconnect();
                    return true;
                }
            }
            /// <summary>
            /// try acquire
            /// </summary>
            /// <param name="connection"></param>
            /// <returns></returns>
            public bool TryAcquire(out IConnection connection)
            {
                var arr = this._arrConnections;
                if (arr == null || arr.Length == 0)
                {
                    connection = null;
                    return false;
                }

                if (arr.Length == 1) connection = arr[0];
                else connection = arr[(Interlocked.Increment(ref this._acquireNumber) & 0x7fffffff) % arr.Length];
                return true;
            }
            /// <summary>
            /// to array
            /// </summary>
            /// <returns></returns>
            public KeyValuePair<string, EndPoint>[] ToArray()
            {
                lock (this) return this._dicRemote.ToArray();
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// connect
            /// </summary>
            /// <param name="name"></param>
            /// <param name="remoteEP"></param>
            private void Connect(string name, EndPoint remoteEP)
            {
                Connect(remoteEP, this._client).ContinueWith(c =>
                {
                    if (c.IsFaulted)
                    {
                        //after 2000~5000ms retry connect
                        TaskEx.Delay(new Random().Next(2000, 5000))
                            .ContinueWith(_ => this.Connect(name, remoteEP)); return;
                    }

                    lock (this)
                    {
                        if (!this._dicRemote.ContainsKey(name))
                        {
                            c.Result.BeginDisconnect(); return;
                        }

                        var connection = c.Result;
                        connection.Disconnected += (conn, ex) =>
                        {
                            lock (this)
                            {
                                if (this._dicConnections.Remove(name))
                                    this._arrConnections = this._dicConnections.Values.ToArray();
                            }
                            //after 100~1500ms retry connect
                            TaskEx.Delay(new Random().Next(100, 1500))
                                .ContinueWith(_ => this.Connect(name, remoteEP));
                        };

                        this._client.RegisterConnection(connection);
                        this._dicConnections[name] = connection;
                        this._arrConnections = this._dicConnections.Values.ToArray();
                    }
                });
            }
            /// <summary>
            /// begin connect
            /// </summary>
            /// <param name="endPoint"></param>
            /// <param name="host"></param>
            /// <exception cref="ArgumentNullException">endPoint is null</exception>
            /// <exception cref="ArgumentNullException">host is null</exception>
            static private Task<IConnection> Connect(EndPoint endPoint, IHost host)
            {
                if (endPoint == null) throw new ArgumentNullException("endPoint");
                if (host == null) throw new ArgumentNullException("host");

                var source = new TaskCompletionSource<IConnection>();
                var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                var e = new SocketAsyncEventArgs();
                e.UserToken = new Tuple<TaskCompletionSource<IConnection>, IHost, Socket>(source, host, socket);
                e.RemoteEndPoint = endPoint;
                e.Completed += ConnectCompleted;

                bool completed = true;
                try { completed = socket.ConnectAsync(e); }
                catch (Exception ex) { source.TrySetException(ex); }
                if (!completed) ThreadPool.QueueUserWorkItem(_ => ConnectCompleted(null, e));

                return source.Task;
            }
            /// <summary>
            /// connect completed
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            static private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
            {
                var t = e.UserToken as Tuple<TaskCompletionSource<IConnection>, IHost, Socket>;
                var source = t.Item1;
                var host = t.Item2;
                var socket = t.Item3;
                var error = e.SocketError;

                e.UserToken = null;
                e.Completed -= ConnectCompleted;
                e.Dispose();

                if (error != SocketError.Success)
                {
                    socket.Close();
                    source.TrySetException(new SocketException((int)error));
                    return;
                }

                socket.NoDelay = true;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                socket.ReceiveBufferSize = host.SocketBufferSize;
                socket.SendBufferSize = host.SocketBufferSize;

                source.TrySetResult(host.NewConnection(socket));
            }
            #endregion
        }
    }
}