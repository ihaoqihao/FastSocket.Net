using System;

namespace Sodao.FastSocket.Server
{
    /// <summary>
    /// udp service
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class AbsUdpService<TMessage> : IUdpService<TMessage>
        where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// on message received
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message"></param>
        public virtual void OnReceived(UdpSession session, TMessage message)
        {
        }
        /// <summary>
        /// on error
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ex"></param>
        public virtual void OnError(UdpSession session, Exception ex)
        {
        }
    }
}