using System;

namespace Sodao.FastSocket.Server.Protocol
{
    /// <summary>
    /// upd protocol
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IUdpProtocol<TMessage> where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// parse protocol message
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        TMessage Parse(ArraySegment<byte> buffer);
    }
}