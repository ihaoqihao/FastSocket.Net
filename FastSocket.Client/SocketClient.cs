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

        private readonly RemoteServerPool _serverPool = null;
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

            this._serverPool = new RemoteServerPool(this);
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

        #region Public Methods
        /// <summary>
        /// try register remote server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="remoteEP"></param>
        /// <param name="initFunc"></param>
        /// <returns></returns>
        public bool TryRegisterRemoteServer(string name, EndPoint remoteEP,
            Func<SendOnceContext, Task> initFunc = null)
        {
            return this._serverPool.TryRegister(name, remoteEP, initFunc);
        }
        /// <summary>
        /// un register remote server
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UnRegisterRemoteServer(string name)
        {
            return this._serverPool.UnRegister(name);
        }
        /// <summary>
        /// remote server to array
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, EndPoint>[] RegisteredServerToArray()
        {
            return this._serverPool.ToArray();
        }
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="request"></param>
        public void Send(Request<TMessage> request)
        {
            IConnection connection = null;
            if (!this._serverPool.TryAcquire(out connection))
            {
                this._pendingQueue.Enqueue(request);
                return;
            }
            connection.BeginSend(request);
        }
        /// <summary>
        /// 产生不重复的seqId
        /// </summary>
        /// <returns></returns>
        public int NextRequestSeqId()
        {
            return Interlocked.Increment(ref this._seqId) & 0x7fffffff;
        }
        /// <summary>
        /// new request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="payload"></param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException"></param>
        /// <param name="onResult"></param>
        /// <returns></returns>
        public Request<TMessage> NewRequest(string name,
            byte[] payload,
            int millisecondsReceiveTimeout,
            Action<Exception> onException,
            Action<TMessage> onResult)
        {
            return new Request<TMessage>(this.NextRequestSeqId(),
                name,
                payload,
                millisecondsReceiveTimeout,
                onException,
                onResult);
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
                r.SentTime = SocketBase.Utils.Date.UtcNow;
                return;
            }

            this._receivingQueue.TryRemove(r.SeqId);

            if (!r.AllowRetry)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try { r.SetException(new RequestException(RequestException.Errors.SendFaild, r.Name)); }
                    catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
                });
                return;
            }

            if (DateTime.UtcNow.Subtract(r.CreatedTime).TotalMilliseconds > this._millisecondsSendTimeout)
            {
                //send time out
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try { r.SetException(new RequestException(RequestException.Errors.PendingSendTimeout, r.Name)); }
                    catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
                });
                return;
            }

            //retry send
            this.Send(r);
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

                        if (dtNow.Subtract(request.CreatedTime).TotalMilliseconds < timeOut)
                        {
                            //try send...
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
                    var arr = this._dic.ToArray().Where(c => c.Value.IsSent() &&
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
        /// remote server pool
        /// </summary>
        private class RemoteServerPool
        {
            #region Members
            /// <summary>
            /// socket client
            /// </summary>
            private readonly SocketClient<TMessage> _client = null;
            /// <summary>
            /// key:nodeId
            /// </summary>
            private readonly Dictionary<int, Node> _dicNode = new Dictionary<int, Node>();
            /// <summary>
            /// key:nodeId
            /// </summary>
            private readonly Dictionary<int, IConnection> _dicConn = new Dictionary<int, IConnection>();

            /// <summary>
            /// <see cref="IConnection"/> array.
            /// </summary>
            private IConnection[] _arrConn = null;
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
            public RemoteServerPool(SocketClient<TMessage> client)
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
            /// <param name="initFunc"></param>
            /// <returns></returns>
            public bool TryRegister(string name, EndPoint remoteEP, Func<SendOnceContext, Task> initFunc)
            {
                var node = new Node(name, remoteEP, initFunc);
                lock (this)
                {
                    if (this._dicNode.Values.FirstOrDefault(c => c.Name == name) != null) return false;
                    this._dicNode[node.Id] = node;
                }

                this.Connect(node);
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

                IConnection connection = null;
                lock (this)
                {
                    var node = this._dicNode.Values.FirstOrDefault(c => c.Name == name);
                    if (node == null) return false;

                    this._dicNode.Remove(node.Id);
                    this._dicConn.TryGetValue(node.Id, out connection);
                }

                if (connection != null) connection.BeginDisconnect();
                return true;
            }
            /// <summary>
            /// try acquire
            /// </summary>
            /// <param name="connection"></param>
            /// <returns></returns>
            public bool TryAcquire(out IConnection connection)
            {
                var arr = this._arrConn;
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
                lock (this)
                {
                    return this._dicNode.Values
                        .Select(c => new KeyValuePair<string, EndPoint>(c.Name, c.RemoteEP))
                        .ToArray();
                }
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// connect
            /// </summary>
            /// <param name="node"></param>
            private void Connect(Node node)
            {
                SocketConnector.Connect(node.RemoteEP)
                    .ContinueWith(task => this.ConnectCallback(node, task));
            }
            /// <summary>
            /// connect callback
            /// </summary>
            /// <param name="node"></param>
            /// <param name="task"></param>
            private void ConnectCallback(Node node, Task<Socket> task)
            {
                bool isActive;
                lock (this) isActive = this._dicNode.ContainsKey(node.Id);

                if (task.IsFaulted)
                {
                    //after 1000~3000ms retry connect
                    if (isActive) TaskEx.Delay(new Random().Next(1000, 3000))
                        .ContinueWith(_ => this.Connect(node));
                    return;
                }

                var socket = task.Result;
                if (!isActive)
                {
                    try { socket.Close(); }
                    catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
                    return;
                }

                socket.NoDelay = true;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                socket.ReceiveBufferSize = this._client.SocketBufferSize;
                socket.SendBufferSize = this._client.SocketBufferSize;
                var connection = this._client.NewConnection(socket);

                connection.Disconnected += (conn, ex) =>
                {
                    bool isExists;
                    lock (this)
                    {
                        isExists = this._dicNode.ContainsKey(node.Id);
                        this._dicConn.Remove(node.Id);
                        this._arrConn = this._dicConn.Values.ToArray();
                    }

                    //after 100~1500ms retry connect
                    if (isExists)
                        TaskEx.Delay(new Random().Next(100, 1500)).ContinueWith(_ => this.Connect(node));
                };
                this._client.RegisterConnection(connection);

                if (node.InitFunc == null)
                {
                    lock (this)
                    {
                        if (this._dicNode.ContainsKey(node.Id))
                        {
                            this._dicConn[node.Id] = connection;
                            this._arrConn = this._dicConn.Values.ToArray();
                            return;
                        }
                    }
                    connection.BeginDisconnect();
                    return;
                }

                node.InitFunc(new SendOnceContext(connection)).ContinueWith(c =>
                {
                    if (c.IsFaulted)
                    {
                        connection.BeginDisconnect(c.Exception.InnerException);
                        return;
                    }

                    lock (this)
                    {
                        if (this._dicNode.ContainsKey(node.Id))
                        {
                            this._dicConn[node.Id] = connection;
                            this._arrConn = this._dicConn.Values.ToArray();
                            return;
                        }
                    }
                    connection.BeginDisconnect();
                });
            }
            #endregion

            /// <summary>
            /// server node
            /// </summary>
            private class Node
            {
                #region Members
                static private int NODEID = 0;

                /// <summary>
                /// id
                /// </summary>
                public readonly int Id;
                /// <summary>
                /// name
                /// </summary>
                public readonly string Name;
                /// <summary>
                /// remote endPoint
                /// </summary>
                public readonly EndPoint RemoteEP;
                /// <summary>
                /// init function
                /// </summary>
                public readonly Func<SendOnceContext, Task> InitFunc;
                #endregion

                #region Constructors
                /// <summary>
                /// new
                /// </summary>
                /// <param name="name"></param>
                /// <param name="remoteEP"></param>
                /// <param name="initFunc"></param>
                /// <exception cref="ArgumentNullException">name is null or empty</exception>
                /// <exception cref="ArgumentNullException">remoteEP</exception>
                public Node(string name, EndPoint remoteEP, Func<SendOnceContext, Task> initFunc)
                {
                    if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
                    if (remoteEP == null) throw new ArgumentNullException("remoteEP");

                    this.Id = Interlocked.Increment(ref NODEID);
                    this.Name = name;
                    this.RemoteEP = remoteEP;
                    this.InitFunc = initFunc;
                }
                #endregion
            }
        }
    }
}