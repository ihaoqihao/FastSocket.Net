using System;

namespace Sodao.FastSocket.SocketBase.Log
{
    /// <summary>
    /// trace listener interface.
    /// </summary>
    public interface ITraceListener
    {
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Error(string message, Exception ex);
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);
    }
}