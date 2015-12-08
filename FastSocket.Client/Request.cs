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
        /// default is don't allow retry send.
        /// </summary>
        internal bool AllowRetry = false;
        /// <summary>
        /// get or set send connection
        /// </summary>
        internal SocketBase.IConnection SendConnection = null;
        /// <summary>
        /// get or set sent time, default is DateTime.MaxValue.
        /// </summary>
        internal DateTime SentTime = DateTime.MaxValue;

        /// <summary>
        /// seqID
        /// </summary>
        public readonly int SeqID;
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
        /// <param name="seqID">seqID</param>
        /// <param name="name">command name</param>
        /// <param name="payload">发送内容</param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException">异常回调</param>
        /// <param name="onResult">结果回调</param>
        /// <exception cref="ArgumentNullException">onException is null</exception>
        /// <exception cref="ArgumentNullException">onResult is null</exception>
        public Request(int seqID, string name, byte[] payload, int millisecondsReceiveTimeout,
            Action<Exception> onException, Action<TMessage> onResult)
            : base(payload)
        {
            if (onException == null) throw new ArgumentNullException("onException");
            if (onResult == null) throw new ArgumentNullException("onResult");

            this.SeqID = seqID;
            this.Name = name;
            this.MillisecondsReceiveTimeout = millisecondsReceiveTimeout;
            this._onException = onException;
            this._onResult = onResult;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// set exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool SetException(Exception ex)
        {
            this._onException(ex);
            return true;
        }
        /// <summary>
        /// set result
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SetResult(TMessage message)
        {
            this._onResult(message);
            return true;
        }
        #endregion
    }
}