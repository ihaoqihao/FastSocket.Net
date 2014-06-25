using System;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// request
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public class Request<TResponse> : SocketBase.Packet where TResponse : Response.IResponse
    {
        #region Members
        /// <summary>
        /// 一致性哈希标识code
        /// </summary>
        public readonly byte[] ConsistentKey = null;
        /// <summary>
        /// seqID
        /// </summary>
        public readonly int SeqID;
        /// <summary>
        /// get command name.
        /// </summary>
        public readonly string CmdName;
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
        private readonly Action<TResponse> _onResult = null;
        #endregion

        #region Constructors
        /// <summary>
        /// new
        /// </summary>
        /// <param name="seqID">seqID</param>
        /// <param name="cmdName">command name</param>
        /// <param name="payload">发送内容</param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException">异常回调</param>
        /// <param name="onResult">结果回调</param>
        public Request(int seqID, string cmdName, byte[] payload, int millisecondsReceiveTimeout, Action<Exception> onException, Action<TResponse> onResult)
            : this(null, seqID, cmdName, payload, millisecondsReceiveTimeout, onException, onResult)
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="consistentKey">一致性哈希标识code, 可为null</param>
        /// <param name="seqID">seqID</param>
        /// <param name="cmdName">command name</param>
        /// <param name="payload">发送内容</param>
        /// <param name="millisecondsReceiveTimeout"></param>
        /// <param name="onException">异常回调</param>
        /// <param name="onResult">结果回调</param>
        /// <exception cref="ArgumentNullException">onException is null</exception>
        /// <exception cref="ArgumentNullException">onResult is null</exception>
        public Request(byte[] consistentKey, int seqID, string cmdName, byte[] payload, int millisecondsReceiveTimeout,
            Action<Exception> onException, Action<TResponse> onResult)
            : base(payload)
        {
            if (onException == null) throw new ArgumentNullException("onException");
            if (onResult == null) throw new ArgumentNullException("onResult");

            this.ConsistentKey = consistentKey;
            this.SeqID = seqID;
            this.CmdName = cmdName;
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
        public bool SetResult(TResponse response)
        {
            this._onResult(response);
            return true;
        }
        #endregion
    }
}