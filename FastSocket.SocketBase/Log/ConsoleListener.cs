using System;

namespace Sodao.FastSocket.SocketBase.Log
{
    /// <summary>
    /// console trace listener
    /// </summary>
    public sealed class ConsoleListener : ITraceListener
    {
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            Console.WriteLine(string.Concat("debug: ", message, Environment.NewLine));
        }
        /// <summary>
        /// error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex)
        {
            Console.WriteLine(string.Concat("error: ", message, Environment.NewLine, ex.ToString(), Environment.NewLine));
        }
        /// <summary>
        /// info
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            Console.WriteLine(string.Concat("info: ", message, Environment.NewLine));
        }
    }
}