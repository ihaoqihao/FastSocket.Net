using System;

namespace Sodao.FastSocket.SocketBase.Log
{
    /// <summary>
    /// trace listener wrapper
    /// </summary>
    public sealed class ListenerWrapper : ITraceListener
    {
        private readonly Action<string> _onDebug = null;
        private readonly Action<string, Exception> _onError = null;
        private readonly Action<string> _onInfo = null;

        /// <summary>
        /// new
        /// </summary>
        /// <param name="onDebug"></param>
        /// <param name="onError"></param>
        /// <param name="onInfo"></param>
        public ListenerWrapper(Action<string> onDebug, Action<string, Exception> onError, Action<string> onInfo)
        {
            if (onDebug == null) throw new ArgumentNullException("onDebug");
            if (onError == null) throw new ArgumentNullException("onError");
            if (onInfo == null) throw new ArgumentNullException("onInfo");

            this._onDebug = onDebug;
            this._onError = onError;
            this._onInfo = onInfo;
        }

        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            this._onDebug(message);
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex)
        {
            this._onError(message, ex);
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            this._onInfo(message);
        }
    }
}