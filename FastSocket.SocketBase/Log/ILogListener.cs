using System;

namespace Sodao.FastSocket.SocketBase.Log
{
    /// <summary>
    /// log interface
    /// </summary>
    public interface ILogListener
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