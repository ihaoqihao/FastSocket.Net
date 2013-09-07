using System;
using System.Collections.Generic;

namespace Sodao.FastSocket.SocketBase.Log
{
    /// <summary>
    /// trace
    /// </summary>
    static public class Trace
    {
        static private readonly List<ITraceListener> _list = new List<ITraceListener>();

        /// <summary>
        /// enable console trace listener
        /// </summary>
        static public void EnableConsole()
        {
            _list.Add(new ConsoleListener());
        }
        /// <summary>
        /// enable diagnostic
        /// </summary>
        static public void EnableDiagnostic()
        {
            _list.Add(new DiagnosticListener());
        }

        /// <summary>
        /// add listener
        /// </summary>
        /// <param name="listener"></param>
        /// <exception cref="ArgumentNullException">listener is null</exception>
        static public void AddListener(ITraceListener listener)
        {
            if (listener == null) throw new ArgumentNullException("listener");
            _list.Add(listener);
        }

        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Debug(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _list.ForEach(c => c.Debug(message));
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentNullException">message is null</exception>
        static public void Info(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _list.ForEach(c => c.Info(message));
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
            _list.ForEach(c => c.Error(message, ex));
        }
    }
}