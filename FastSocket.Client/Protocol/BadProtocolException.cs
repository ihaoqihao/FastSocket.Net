
namespace Sodao.FastSocket.Client.Protocol
{
    /// <summary>
    /// bad protocol exception
    /// </summary>
    public sealed class BadProtocolException : System.ApplicationException
    {
        /// <summary>
        /// new
        /// </summary>
        public BadProtocolException()
            : base("bad protocol.")
        {
        }
        /// <summary>
        /// new
        /// </summary>
        /// <param name="message"></param>
        public BadProtocolException(string message)
            : base(message)
        {
        }
    }
}