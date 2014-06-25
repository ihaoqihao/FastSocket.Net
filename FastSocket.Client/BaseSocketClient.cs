using Sodao.FastSocket.SocketBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// socket client
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class BaseSocketClient<TResponse> : SocketBase.BaseHost where TResponse : class, Response.IResponse
    {
        #region Private Members
        private int _requestSeqId = 0;
        private readonly Protocol.IProtocol<TResponse> _protocol = null;

        private readonly int _millisecondsSendTimeout;
        private readonly int _millisecondsReceiveTimeout;

        private readonly PendingSendQueue _pendingQueue = null;
        private readonly ReceivingCollection _receivingCollection = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="protocol"></param>
        public BaseSocketClient(Protocol.IProtocol<TResponse> protocol)
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
        public BaseSocketClient(Protocol.IProtocol<TResponse> protocol,
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

            this._pendingQueue = new PendingSendQueue(this.ScanningPendingRequest);
            this._receivingCollection = new ReceivingCollection(this.ReceivingTimeout);
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

        #region Private Methods
        /// <summary>
        /// scanning pending request.
        /// </summary>
        /// <param name="request"></param>
        private void ScanningPendingRequest(Request<TResponse> request)
        {
            if (DateTime.UtcNow.Subtract(request.CreatedTime).TotalMilliseconds < this._millisecondsSendTimeout)
            {
                this.Send(request);
                return;
            }

            //send time out
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { request.SetException(new RequestException(RequestException.Errors.PendingSendTimeout, request.CmdName)); }
                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        /// <summary>
        /// receiving time out
        /// </summary>
        /// <param name="request"></param>
        private void ReceivingTimeout(Request<TResponse> request)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { request.SetException(new RequestException(RequestException.Errors.ReceiveTimeout, request.CmdName)); }
                catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
            });
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 处理未知的response
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="response"></param>
        protected virtual void HandleUnknowResponse(IConnection connection, TResponse response)
        {
        }
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="request"></param>
        protected abstract void Send(Request<TResponse> request);
        /// <summary>
        /// enqueue to pending queue
        /// </summary>
        /// <param name="request"></param>
        protected void EnqueueToPendingQueue(Request<TResponse> request)
        {
            this._pendingQueue.Enqueue(request);
        }
        /// <summary>
        /// dequeue from pending queue
        /// </summary>
        /// <returns></returns>
        protected Request<TResponse> DequeueFromPendingQueue()
        {
            return this._pendingQueue.TryDequeue();
        }
        /// <summary>
        /// dequeue all from pending queue.
        /// </summary>
        /// <returns></returns>
        protected Request<TResponse>[] DequeueAllFromPendingQueue()
        {
            return this._pendingQueue.DequeueAll();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 产生不重复的seqID
        /// </summary>
        /// <returns></returns>
        public int NextRequestSeqID()
        {
            return Interlocked.Increment(ref this._requestSeqId) & 0x7fffffff;
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
            this._receivingCollection.Add(packet as Request<TResponse>);
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

            var request = packet as Request<TResponse>;
            if (request == null) return;

            if (isSuccess)
            {
                request.SentTime = DateTime.UtcNow;
                return;
            }

            request.SentTime = DateTime.MaxValue;
            if (this._receivingCollection.TryRemove(request.SeqID) == null) return;
            if (DateTime.UtcNow.Subtract(request.CreatedTime).TotalMilliseconds < this._millisecondsSendTimeout)
            {
                this.Send(request);
                return;
            }

            //send time out
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { request.SetException(new RequestException(RequestException.Errors.PendingSendTimeout, request.CmdName)); }
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
            TResponse response = null;
            try { response = this._protocol.FindResponse(connection, e.Buffer, out readlength); }
            catch (Exception ex)
            {
                base.OnConnectionError(connection, ex);
                connection.BeginDisconnect(ex);
                e.SetReadlength(e.Buffer.Count);
                return;
            }

            if (response != null)
            {
                var request = this._receivingCollection.TryRemove(response.SeqID);
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        if (request == null) this.HandleUnknowResponse(connection, response);
                        else request.SetResult(response);
                    }
                    catch (Exception ex) { SocketBase.Log.Trace.Error(ex.Message, ex); }
                });
            }

            e.SetReadlength(readlength);
        }
        #endregion

        #region PendingSendQueue
        /// <summary>
        /// pending send queue
        /// </summary>
        private class PendingSendQueue
        {
            #region Private Members
            private readonly Timer _timer = null;
            private readonly ConcurrentQueue<Request<TResponse>> _queue = new ConcurrentQueue<Request<TResponse>>();
            private readonly Action<Request<TResponse>> _onScanning = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="onScanning"></param>
            public PendingSendQueue(Action<Request<TResponse>> onScanning)
            {
                if (onScanning == null) throw new ArgumentNullException("onScanning");
                this._onScanning = onScanning;
                this._timer = new Timer(this.TimerCallback, null, 200, 0);
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// timer callback
            /// </summary>
            /// <param name="state"></param>
            private void TimerCallback(object state)
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                int count = this._queue.Count;
                while (count-- > 0)
                {
                    Request<TResponse> request;
                    if (this._queue.TryDequeue(out request)) this._onScanning(request);
                }
                this._timer.Change(200, 0);
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// 入列
            /// </summary>
            /// <param name="request"></param>
            /// <exception cref="ArgumentNullException">request is null</exception>
            public void Enqueue(Request<TResponse> request)
            {
                if (request == null) throw new ArgumentNullException("request");
                this._queue.Enqueue(request);
            }
            /// <summary>
            /// try dequeue
            /// </summary>
            /// <returns></returns>
            public Request<TResponse> TryDequeue()
            {
                Request<TResponse> request;
                if (this._queue.TryDequeue(out request)) return request;
                return null;
            }
            /// <summary>
            /// 出列全部
            /// </summary>
            /// <returns></returns>
            public Request<TResponse>[] DequeueAll()
            {
                int count = this._queue.Count;
                List<Request<TResponse>> list = null;
                while (count-- > 0)
                {
                    Request<TResponse> request;
                    if (!this._queue.TryDequeue(out request)) break;

                    if (list == null) list = new List<Request<TResponse>>();
                    list.Add(request);
                }

                if (list == null) return new Request<TResponse>[0];
                return list.ToArray();
            }
            #endregion
        }
        #endregion

        #region ReceivingCollection
        /// <summary>
        /// receiving collection
        /// </summary>
        private class ReceivingCollection
        {
            #region Private Members
            private readonly Timer _timer = null;
            private readonly ConcurrentDictionary<int, Request<TResponse>> _dic = new ConcurrentDictionary<int, Request<TResponse>>();
            private readonly Action<Request<TResponse>> _onTimeout = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="onTimeout"></param>
            public ReceivingCollection(Action<Request<TResponse>> onTimeout)
            {
                if (onTimeout == null) throw new ArgumentNullException("timeoutAction");
                this._onTimeout = onTimeout;
                this._timer = new Timer(this.TimerCallback, null, 500, 0);
            }
            #endregion

            #region Private Methods
            /// <summary>
            /// timer callback
            /// </summary>
            /// <param name="state"></param>
            private void TimerCallback(object state)
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (this._dic.Count > 0)
                {
                    var dtNow = DateTime.UtcNow;
                    var arr = this._dic.ToArray().Where(c => dtNow.Subtract(c.Value.SentTime).TotalMilliseconds >
                        c.Value.MillisecondsReceiveTimeout).ToArray();

                    for (int i = 0, l = arr.Length; i < l; i++)
                    {
                        Request<TResponse> removed;
                        if (this._dic.TryRemove(arr[i].Key, out removed)) this._onTimeout(removed);
                    }
                }
                this._timer.Change(500, 0);
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// add
            /// </summary>
            /// <param name="request"></param>
            public void Add(Request<TResponse> request)
            {
                this._dic.TryAdd(request.SeqID, request);
            }
            /// <summary>
            /// remove
            /// </summary>
            /// <param name="seqID"></param>
            /// <returns></returns>
            public Request<TResponse> TryRemove(int seqID)
            {
                Request<TResponse> removed = null;
                this._dic.TryRemove(seqID, out removed);
                return removed;
            }
            #endregion
        }
        #endregion
    }
}