using System;
using System.Collections.Generic;

namespace Sodao.FastSocket.SocketBase.Log
{
    /// <summary>
    /// logger
    /// </summary>
    static public class Logger
    {
        static private readonly List<ILogListener> _listListeners = new List<ILogListener>();

        /// <summary>
        /// static new
        /// </summary>
        static Logger()
        {
            _listListeners.Add(new ConsoleLogListener());
        }

        /// <summary>
        /// add listener
        /// </summary>
        /// <param name="listener"></param>
        /// <exception cref="ArgumentNullException">listener is null</exception>
        static public void AddListenner(ILogListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener");
            _listListeners.Add(listener);
        }

        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Debug(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _listListeners.ForEach(c => c.Debug(message));
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Info(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _listListeners.ForEach(c => c.Info(message));
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Error(string message, Exception ex)
        {
            if (message == null) throw new ArgumentNullException("message");
            _listListeners.ForEach(c => c.Error(message, ex));
        }
    }
}