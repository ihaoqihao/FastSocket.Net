using System;

namespace Sodao.FastSocket.Client
{
    /// <summary>
    /// socket request exception
    /// </summary>
    public sealed class RequestException : ApplicationException
    {
        /// <summary>
        /// error
        /// </summary>
        public readonly Errors Error;
        /// <summary>
        /// get cmdName
        /// </summary>
        public readonly string CmdName;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="error"></param>
        /// <param name="cmdName"></param>
        public RequestException(Errors error, string cmdName)
            : base(string.Concat("errorType:", error.ToString(), " cmdName:", cmdName ?? string.Empty))
        {
            this.Error = error;
        }

        /// <summary>
        /// error type enum
        /// </summary>
        public enum Errors : byte
        {
            /// <summary>
            /// 未知
            /// </summary>
            Unknow = 0,
            /// <summary>
            /// 等待发送超时
            /// </summary>
            PendingSendTimeout = 1,
            /// <summary>
            /// 接收超时
            /// </summary>
            ReceiveTimeout = 2
        }
    }
}