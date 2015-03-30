using System;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// request
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class Request<TMessage> : SocketBase.Packet where TMessage : Messaging.IMessage
    {
        #region Members
        /// <summary>
        /// default is allow retry send.
        /// </summary>
        internal bool AllowRetry = true;
        /// <summary>
        /// seqId
        /// </summary>
        public readonly int SeqId;
        /// <summary>
        /// get request name.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// get or set receive time out
        /// </summary>
        public readonly int MillisecondsReceiveTimeout;

        /// <summary>
        /// 异常回调
        /// </summary>
        private readonly Action<Exception> _onException = null;
        /// <summary>
        /// 结果回调
        /// </summary>
        private readonly Action<TMessage> _onResult = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqId">seqID</param>
        /// <param name="name">command name</param>
        /// <param name="payload">发送内容</param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException">异常回调</param>
        /// <param name="onResult">结果回调</param>
        /// <exception cref="ArgumentNullException">onException is null</exception>
        /// <exception cref="ArgumentNullException">onResult is null</exception>
        public Request(int seqId, string name, byte[] payload, int millisecondsReceiveTimeout,
            Action<Exception> onException, Action<TMessage> onResult)
            : base(payload)
        {
            if (onException == null) throw new ArgumentNullException("onException");
            if (onResult == null) throw new ArgumentNullException("onResult");

            this.SeqId = seqId;
            this.Name = name;
            this.MillisecondsReceiveTimeout = millisecondsReceiveTimeout;
            this._onException = onException;
            this._onResult = onResult;
            this.SentTime = DateTime.MaxValue;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// get sent time
        /// </summary>
        public DateTime SentTime
        {
            get;
            internal set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// set Exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool SetException(Exception ex)
        {
            this._onException(ex);
            return true;
        }
        /// <summary>
        /// set Result
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public bool SetResult(TMessage response)
        {
            this._onResult(response);
            return true;
        }
        #endregion
    }
}