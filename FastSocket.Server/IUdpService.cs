using System;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// udp service interface.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IUdpService<TMessage> where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// on message received
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message"></param>
        void OnReceived(UdpSession session, TMessage message);
        /// <summary>
        /// on error.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ex"></param>
        void OnError(UdpSession session, Exception ex);
    }
}