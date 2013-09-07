using System;

namespace Sodao.FastSocket.SocketBase.Log
{
    /// <summary>
    /// diagnostic listener
    /// </summary>
    public sealed class DiagnosticListener : ITraceListener
    {
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            System.Diagnostics.Trace.TraceInformation(message);
        }
    }
}