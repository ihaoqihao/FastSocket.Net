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
        #region Events
        /// <summary>
        /// received unknow message
        /// </summary>
        public event Action<SocketBase.IConnection, TMessage> ReceivedUnknowMessage;
        #endregion

        #region Private Members
        private int _seqID = 0;
        private readonly Protocol.IProtocol<TMessage> _protocol = null;

        private readonly int _millisecondsSendTimeout;
        private readonly int _millisecondsReceiveTimeout;

        private readonly PendingSendQueue _pendingQueue = null;
        private readonly ReceivingQueue _receivingQueue = null;

        private readonly EndPointManager _endPointManager = null;
        private readonly IConnectionPool _connectionPool = null;
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

            if (protocol.IsAsync) this._connectionPool = new AsyncPool();
            else this._connectionPool = new SyncPool();

            this._millisecondsSendTimeout = millisecondsSendTimeout;
            this._millisecondsReceiveTimeout = millisecondsReceiveTimeout;

            this._pendingQueue = new PendingSendQueue(this);
            this._receivingQueue = new ReceivingQueue(this);

            this._endPointManager = new EndPointManager(this);
            this._endPointManager.Connected += this.OnEndPointConnected;
            this._endPointManager.Already += this.OnEndPointAlready;
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
        /// try register endPoint
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arrRemoteEP"></param>
        /// <param name="initFunc"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">socketClient</exception>
        public bool TryRegisterEndPoint(string name, EndPoint[] arrRemoteEP, Func<SocketBase.IConnection, Task> initFunc = null)
        {
            return this._endPointManager.TryRegister(name, arrRemoteEP, initFunc);
        }
        /// <summary>
        /// un register endPoint
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">socketClient</exception>
        public bool UnRegisterEndPoint(string name)
        {
            return this._endPointManager.UnRegister(name);
        }
        /// <summary>
        /// get all registered endPoint
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string, EndPoint[]>[] GetAllRegisteredEndPoint()
        {
            return this._endPointManager.ToArray();
        }
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ArgumentNullException">request is null.</exception>
        public void Send(Request<TMessage> request)
        {
            if (request == null) throw new ArgumentNullException("request");

            request.AllowRetry = true;
            SocketBase.IConnection connection = null;
            if (this._connectionPool.TryAcquire(out connection))
            {
                connection.BeginSend(request);
                return;
            }
            this._pendingQueue.Enqueue(request);
        }
        /// <summary>
        /// send packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">packet is null.</exception>
        public bool Send(SocketBase.Packet packet)
        {
            if (packet == null) throw new ArgumentNullException("packet");

            SocketBase.IConnection connection = null;
            if (!this._connectionPool.TryAcquire(out connection)) return false;

            connection.BeginSend(packet);
            return true;
        }
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        /// <exception cref="ArgumentNullException">connection is null.</exception>
        /// <exception cref="ArgumentNullException">request is null.</exception>
        public void Send(SocketBase.IConnection connection, Request<TMessage> request)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (request == null) throw new ArgumentNullException("request");

            connection.BeginSend(request);
        }
        /// <summary>
        /// 产生不重复的seqID
        /// </summary>
        /// <returns></returns>
        public int NextRequestSeqID()
        {
            return Interlocked.Increment(ref this._seqID) & 0x7fffffff;
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
        public Request<TMessage> NewRequest(string name, byte[] payload,
            int millisecondsReceiveTimeout,
            Action<Exception> onException, Action<TMessage> onResult)
        {
            var seqID = this._protocol.IsAsync ? this.NextRequestSeqID() : this._protocol.DefaultSyncSeqID;
            return new Request<TMessage>(seqID, name, payload,
                millisecondsReceiveTimeout, onException, onResult);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// try send next request
        /// </summary>
        protected void TrySendNext()
        {
            Request<TMessage> request = null;
            if (this._pendingQueue.TryDequeue(out request)) this.Send(request);
        }
        /// <summary>
        /// endPoint connected
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connection"></param>
        protected virtual void OnEndPointConnected(string name, SocketBase.IConnection connection)
        {
            base.RegisterConnection(connection);
        }
        /// <summary>
        /// endPoint already available
        /// </summary>
        /// <param name="name"></param>
        /// <param name="connection"></param>
        protected virtual void OnEndPointAlready(string name, SocketBase.IConnection connection)
        {
            this._connectionPool.Register(connection);
        }
        /// <summary>
        /// on pending send timeout
        /// </summary>
        /// <param name="request"></param>
        protected virtual void OnPendingSendTimeout(Request<TMessage> request)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { request.SetException(new RequestException(RequestException.Errors.PendingSendTimeout, request.Name)); }
                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        /// <summary>
        /// on request sent
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        protected virtual void OnSent(SocketBase.IConnection connection, Request<TMessage> request)
        {
        }
        /// <summary>
        /// on send failed
        /// </summary>
        /// <param name="request"></param>
        protected virtual void OnSendFailed(Request<TMessage> request)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { request.SetException(new RequestException(RequestException.Errors.SendFaild, request.Name)); }
                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        /// <summary>
        /// on request received
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        /// <param name="message"></param>
        protected virtual void OnReceived(SocketBase.IConnection connection, Request<TMessage> request, TMessage message)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { request.SetResult(message); }
                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });

            if (!this._protocol.IsAsync)
            {
                //release connection
                this._connectionPool.Release(connection);
                //try send next request
                this.TrySendNext();
            }
        }
        /// <summary>
        /// on received unknow message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        protected virtual void OnReceivedUnknowMessage(SocketBase.IConnection connection, TMessage message)
        {
            if (this.ReceivedUnknowMessage != null)
                this.ReceivedUnknowMessage(connection, message);
        }
        /// <summary>
        /// on receive timeout
        /// </summary>
        /// <param name="request"></param>
        protected virtual void OnReceiveTimeout(Request<TMessage> request)
        {
            if (!this._protocol.IsAsync)
                request.SendConnection.BeginDisconnect();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { request.SetException(new RequestException(RequestException.Errors.ReceiveTimeout, request.Name)); }
                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// OnConnected
        /// </summary>
        /// <param name="connection"></param>
        protected override void OnConnected(SocketBase.IConnection connection)
        {
            base.OnConnected(connection);
            connection.BeginReceive();//异步开始接收数据
        }
        /// <summary>
        /// on disconnected
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        protected override void OnDisconnected(SocketBase.IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);
            this._connectionPool.Destroy(connection);
        }
        /// <summary>
        /// OnStartSending
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        protected override void OnStartSending(SocketBase.IConnection connection, SocketBase.Packet packet)
        {
            base.OnStartSending(connection, packet);

            var request = packet as Request<TMessage>;
            if (request == null) return;

            request.SendConnection = connection;
            this._receivingQueue.TryAdd(request);
        }
        /// <summary>
        /// OnSendCallback
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="packet"></param>
        /// <param name="isSuccess"></param>
        protected override void OnSendCallback(SocketBase.IConnection connection, SocketBase.Packet packet, bool isSuccess)
        {
            base.OnSendCallback(connection, packet, isSuccess);

            var request = packet as Request<TMessage>;
            if (request == null) return;

            if (isSuccess)
            {
                request.SentTime = SocketBase.Utils.Date.UtcNow;
                this.OnSent(connection, request);
                return;
            }

            Request<TMessage> removed;
            if (this._receivingQueue.TryRemove(connection.ConnectionID, request.SeqID, out removed))
                removed.SendConnection = null;

            if (!request.AllowRetry)
            {
                this.OnSendFailed(request);
                return;
            }

            if (SocketBase.Utils.Date.UtcNow.Subtract(request.CreatedTime).TotalMilliseconds > this._millisecondsSendTimeout)
            {
                //send time out
                this.OnPendingSendTimeout(request);
                return;
            }

            //retry send
            this.Send(request);
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="e"></param>
        protected override void OnMessageReceived(SocketBase.IConnection connection,
            SocketBase.MessageReceivedEventArgs e)
        {
            base.OnMessageReceived(connection, e);

            //process message
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
                Request<TMessage> request = null;
                if (this._receivingQueue.TryRemove(connection.ConnectionID, message.SeqID, out request))
                    this.OnReceived(connection, request, message);
                else this.OnReceivedUnknowMessage(connection, message);
            }

            //continue receiveing..
            e.SetReadlength(readlength);
        }
        /// <summary>
        /// stop
        /// </summary>
        public override void Start()
        {
            this._endPointManager.Start();
        }
        /// <summary>
        /// stop
        /// </summary>
        public override void Stop()
        {
            this._endPointManager.Stop();
            base.Stop();
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
                        this._client.OnPendingSendTimeout(request);
                    }

                    this._timer.Change(500, 500);
                }, null, 500, 500);
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
            /// <summary>
            /// TryDequeue
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public bool TryDequeue(out Request<TMessage> request)
            {
                return this._queue.TryDequeue(out request);
            }
            #endregion
        }

        /// <summary>
        /// receiving queue
        /// </summary>
        private class ReceivingQueue
        {
            #region Private Members
            /// <summary>
            /// socket client
            /// </summary>
            private readonly SocketClient<TMessage> _client = null;
            /// <summary>
            /// key:connectionID:request.SeqID
            /// </summary>
            private readonly ConcurrentDictionary<string, Request<TMessage>> _dic =
                new ConcurrentDictionary<string, Request<TMessage>>();
            /// <summary>
            /// timer for check receive timeout
            /// </summary>
            private readonly Timer _timer = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="client"></param>
            public ReceivingQueue(SocketClient<TMessage> client)
            {
                this._client = client;

                this._timer = new Timer(_ =>
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
                            this._client.OnReceiveTimeout(request);
                    }

                    this._timer.Change(500, 500);
                }, null, 500, 500);
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// to key
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            private string ToKey(Request<TMessage> request)
            {
                if (request.SendConnection == null) throw new ArgumentNullException("request.SendConnection");
                return this.ToKey(request.SendConnection.ConnectionID, request.SeqID);
            }
            /// <summary>
            /// to key
            /// </summary>
            /// <param name="connectionID"></param>
            /// <param name="seqID"></param>
            /// <returns></returns>
            private string ToKey(long connectionID, int seqID)
            {
                return string.Concat(connectionID.ToString(), "/", seqID.ToString());
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
                return this._dic.TryAdd(this.ToKey(request), request);
            }
            /// <summary>
            /// try remove
            /// </summary>
            /// <param name="connectionID"></param>
            /// <param name="seqID"></param>
            /// <param name="request"></param>
            /// <returns></returns>
            public bool TryRemove(long connectionID, int seqID, out Request<TMessage> request)
            {
                return this._dic.TryRemove(this.ToKey(connectionID, seqID), out request);
            }
            #endregion
        }

        /// <summary>
        /// node info
        /// </summary>
        private class NodeInfo
        {
            #region Members
            /// <summary>
            /// name
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// remote endPoint array
            /// </summary>
            public readonly EndPoint[] ArrRemoteEP;
            /// <summary>
            /// init function
            /// </summary>
            public readonly Func<SocketBase.IConnection, Task> InitFunc;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="name"></param>
            /// <param name="arrRemoteEP"></param>
            /// <param name="initFunc"></param>
            /// <exception cref="ArgumentNullException">name is null or empty</exception>
            /// <exception cref="ArgumentNullException">arrRemoteEP is null or empty</exception>
            public NodeInfo(string name, EndPoint[] arrRemoteEP,
                Func<SocketBase.IConnection, Task> initFunc)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
                if (arrRemoteEP == null || arrRemoteEP.Length == 0) throw new ArgumentNullException("arrRemoteEP");

                this.Name = name;
                this.ArrRemoteEP = arrRemoteEP;
                this.InitFunc = initFunc;
            }
            #endregion
        }

        /// <summary>
        /// server node
        /// </summary>
        private class Node : IDisposable
        {
            #region Members
            static private int NODE_ID = 0;

            private readonly SocketBase.IHost _host = null;
            private readonly Action<Node, SocketBase.IConnection> _connectedCallback;
            private readonly Action<Node, SocketBase.IConnection> _alreadyCallback;

            private bool _isdisposed = false;
            private SocketBase.IConnection _connection = null;

            /// <summary>
            /// id
            /// </summary>
            public readonly int ID;
            /// <summary>
            /// node info
            /// </summary>
            public readonly NodeInfo Info;
            #endregion

            #region Constructors
            /// <summary>
            /// free
            /// </summary>
            ~Node()
            {
                this.Dispose();
            }
            /// <summary>
            /// new
            /// </summary>
            /// <param name="info"></param>
            /// <param name="host"></param>
            /// <param name="connectedCallback"></param>
            /// <param name="alreadyCallback"></param>
            public Node(NodeInfo info, SocketBase.IHost host,
                Action<Node, SocketBase.IConnection> connectedCallback,
                Action<Node, SocketBase.IConnection> alreadyCallback)
            {
                if (info == null) throw new ArgumentNullException("info");
                if (host == null) throw new ArgumentNullException("host");
                if (connectedCallback == null) throw new ArgumentNullException("connectedCallback");
                if (alreadyCallback == null) throw new ArgumentNullException("alreadyCallback");

                this.ID = Interlocked.Increment(ref NODE_ID);
                this.Info = info;
                this._host = host;
                this._connectedCallback = connectedCallback;
                this._alreadyCallback = alreadyCallback;

                this.Connect();
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// begin connect
            /// </summary>
            private void Connect()
            {
                SocketConnector.Connect(this.Info.ArrRemoteEP.Length == 1 ?
                    this.Info.ArrRemoteEP[0] :
                    this.Info.ArrRemoteEP[(Guid.NewGuid().GetHashCode() & int.MaxValue) % this.Info.ArrRemoteEP.Length])
                    .ContinueWith(t => this.ConnectCallback(t));
            }
            /// <summary>
            /// connect callback
            /// </summary>
            /// <param name="t"></param>
            private void ConnectCallback(Task<Socket> t)
            {
                if (t.IsFaulted)
                {
                    lock (this) { if (this._isdisposed) return; }
                    SocketBase.Utils.TaskEx.Delay(new Random().Next(500, 1500)).ContinueWith(_ => this.Connect());
                    return;
                }

                var connection = this._host.NewConnection(t.Result);
                connection.Disconnected += (conn, ex) =>
                {
                    lock (this)
                    {
                        this._connection = null;
                        if (this._isdisposed) return;
                    }
                    SocketBase.Utils.TaskEx.Delay(new Random().Next(100, 1000)).ContinueWith(_ => this.Connect());
                };

                //fire node connected event.
                this._connectedCallback(this, connection);

                if (this.Info.InitFunc == null)
                {
                    lock (this)
                    {
                        if (this._isdisposed)
                        {
                            connection.BeginDisconnect();
                            return;
                        }
                        this._connection = connection;
                    }
                    //fire node already event.
                    this._alreadyCallback(this, connection);
                    return;
                }

                this.Info.InitFunc(connection).ContinueWith(c =>
                {
                    if (c.IsFaulted)
                    {
                        connection.BeginDisconnect(c.Exception.InnerException);
                        return;
                    }

                    lock (this)
                    {
                        if (this._isdisposed)
                        {
                            connection.BeginDisconnect();
                            return;
                        }
                        this._connection = connection;
                    }
                    //fire node already event.
                    this._alreadyCallback(this, connection);
                });
            }
            #endregion

            #region IDisposable Members
            /// <summary>
            /// dispose
            /// </summary>
            public void Dispose()
            {
                SocketBase.IConnection exists = null;
                lock (this)
                {
                    if (this._isdisposed) return;
                    this._isdisposed = true;

                    exists = this._connection;
                    this._connection = null;
                }
                if (exists != null) exists.BeginDisconnect();
                GC.SuppressFinalize(this);
            }
            #endregion
        }

        /// <summary>
        /// endPoint manager
        /// </summary>
        private class EndPointManager
        {
            #region Events
            /// <summary>
            /// node connected event
            /// </summary>
            public event Action<string, SocketBase.IConnection> Connected;
            /// <summary>
            /// node already event
            /// </summary>
            public event Action<string, SocketBase.IConnection> Already;
            #endregion

            #region Members
            /// <summary>
            /// host
            /// </summary>
            private readonly SocketBase.IHost _host = null;
            /// <summary>
            /// key:node name
            /// </summary>
            private readonly Dictionary<string, NodeInfo> _dicNodeInfo =
                new Dictionary<string, NodeInfo>();
            /// <summary>
            /// key:node id
            /// </summary>
            private readonly Dictionary<int, Node> _dicNodes =
                new Dictionary<int, Node>();
            /// <summary>
            /// true is runing
            /// </summary>
            private bool _isRuning = true;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="host"></param>
            public EndPointManager(SocketBase.IHost host)
            {
                this._host = host;
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// try register
            /// </summary>
            /// <param name="name"></param>
            /// <param name="arrRemoteEP"></param>
            /// <param name="initFunc"></param>
            /// <returns></returns>
            public bool TryRegister(string name, EndPoint[] arrRemoteEP, Func<SocketBase.IConnection, Task> initFunc)
            {
                lock (this)
                {
                    if (this._dicNodeInfo.ContainsKey(name)) return false;
                    var nodeInfo = new NodeInfo(name, arrRemoteEP, initFunc);
                    this._dicNodeInfo[name] = nodeInfo;

                    if (this._isRuning)
                    {
                        var node = new Node(nodeInfo, this._host, this.OnNodeConnected, this.OnNodeAlready);
                        this._dicNodes[node.ID] = node;
                    }
                    return true;
                }
            }
            /// <summary>
            /// un register
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public bool UnRegister(string name)
            {
                KeyValuePair<int, Node>[] arrRemoved = null;
                lock (this)
                {
                    if (!this._dicNodeInfo.Remove(name)) return false;
                    arrRemoved = this._dicNodes.Where(c => c.Value.Info.Name == name).ToArray();
                    foreach (var child in arrRemoved)
                        this._dicNodes.Remove(child.Key);
                }
                if (arrRemoved != null)
                    foreach (var child in arrRemoved) child.Value.Dispose();

                return true;
            }
            /// <summary>
            /// to array
            /// </summary>
            /// <returns></returns>
            public KeyValuePair<string, EndPoint[]>[] ToArray()
            {
                lock (this)
                    return this._dicNodeInfo.Values.Select(c =>
                        new KeyValuePair<string, EndPoint[]>(c.Name, c.ArrRemoteEP)).ToArray();
            }
            /// <summary>
            /// start
            /// </summary>
            public void Start()
            {
                lock (this)
                {
                    if (this._isRuning) return;
                    this._isRuning = true;
                    foreach (var info in this._dicNodeInfo.Values)
                    {
                        var node = new Node(info, this._host, this.OnNodeConnected, this.OnNodeAlready);
                        this._dicNodes[node.ID] = node;
                    }
                }
            }
            /// <summary>
            /// stop
            /// </summary>
            public void Stop()
            {
                Node[] arrNodes = null;
                lock (this)
                {
                    if (!this._isRuning) return;
                    this._isRuning = false;
                    arrNodes = this._dicNodes.Values.ToArray();
                    this._dicNodes.Clear();
                }
                if (arrNodes == null || arrNodes.Length == 0) return;
                foreach (var node in arrNodes) node.Dispose();
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// on node connected
            /// </summary>
            /// <param name="node"></param>
            /// <param name="connection"></param>
            private void OnNodeConnected(Node node, SocketBase.IConnection connection)
            {
                if (this.Connected == null) return;
                this.Connected(node.Info.Name, connection);
            }
            /// <summary>
            /// on node already
            /// </summary>
            /// <param name="node"></param>
            /// <param name="connection"></param>
            private void OnNodeAlready(Node node, SocketBase.IConnection connection)
            {
                if (this.Already == null) return;
                this.Already(node.Info.Name, connection);
            }
            #endregion
        }

        /// <summary>
        /// connection pool interface
        /// </summary>
        private interface IConnectionPool
        {
            #region Public Methods
            /// <summary>
            /// register
            /// </summary>
            /// <param name="connection"></param>
            void Register(SocketBase.IConnection connection);
            /// <summary>
            /// try acquire <see cref="SocketBase.IConnection"/>
            /// </summary>
            /// <param name="connection"></param>
            /// <returns></returns>
            bool TryAcquire(out SocketBase.IConnection connection);
            /// <summary>
            /// release
            /// </summary>
            /// <param name="connection"></param>
            void Release(SocketBase.IConnection connection);
            /// <summary>
            /// destroy
            /// </summary>
            /// <param name="connection"></param>
            void Destroy(SocketBase.IConnection connection);
            #endregion
        }

        /// <summary>
        /// async connection pool
        /// </summary>
        public sealed class AsyncPool : IConnectionPool
        {
            #region Private Members
            private readonly List<SocketBase.IConnection> _list = new List<SocketBase.IConnection>();
            private SocketBase.IConnection[] _arr = null;
            private int _acquireNumber = 0;
            #endregion

            #region Public Methods
            /// <summary>
            /// register
            /// </summary>
            /// <param name="connection"></param>
            public void Register(SocketBase.IConnection connection)
            {
                if (connection == null) throw new ArgumentNullException("connection");

                lock (this)
                {
                    if (this._list.Contains(connection)) return;

                    this._list.Add(connection);
                    this._arr = this._list.ToArray();
                }
            }
            /// <summary>
            /// try acquire
            /// </summary>
            /// <param name="connection"></param>
            /// <returns></returns>
            public bool TryAcquire(out SocketBase.IConnection connection)
            {
                var arr = this._arr;
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
            /// release
            /// </summary>
            /// <param name="connection"></param>
            public void Release(SocketBase.IConnection connection)
            {
            }
            /// <summary>
            /// destroy
            /// </summary>
            /// <param name="connection"></param>
            public void Destroy(SocketBase.IConnection connection)
            {
                if (connection == null) throw new ArgumentNullException("connection");

                lock (this)
                {
                    if (this._list.Remove(connection)) this._arr = this._list.ToArray();
                }
            }
            #endregion
        }

        /// <summary>
        /// sync connection pool
        /// </summary>
        public sealed class SyncPool : IConnectionPool
        {
            #region Private Members
            private readonly ConcurrentDictionary<long, SocketBase.IConnection> _dic =
                new ConcurrentDictionary<long, SocketBase.IConnection>();
            private readonly ConcurrentStack<SocketBase.IConnection> _stack =
                new ConcurrentStack<SocketBase.IConnection>();
            #endregion

            #region Public Methods
            /// <summary>
            /// register
            /// </summary>
            /// <param name="connection"></param>
            public void Register(SocketBase.IConnection connection)
            {
                if (this._dic.TryAdd(connection.ConnectionID, connection))
                    this._stack.Push(connection);
            }
            /// <summary>
            /// try acquire
            /// </summary>
            /// <param name="connection"></param>
            /// <returns></returns>
            public bool TryAcquire(out SocketBase.IConnection connection)
            {
                return this._stack.TryPop(out connection);
            }
            /// <summary>
            /// release
            /// </summary>
            /// <param name="connection"></param>
            public void Release(SocketBase.IConnection connection)
            {
                if (this._dic.ContainsKey(connection.ConnectionID))
                    this._stack.Push(connection);
            }
            /// <summary>
            /// destroy
            /// </summary>
            /// <param name="connection"></param>
            public void Destroy(SocketBase.IConnection connection)
            {
                SocketBase.IConnection exists = null;
                this._dic.TryRemove(connection.ConnectionID, out exists);
            }
            #endregion
        }
    }
}