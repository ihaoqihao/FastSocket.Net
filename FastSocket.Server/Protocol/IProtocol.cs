using Sodao.FastSocket.SocketBase;
using System;

namespace Sodao.FastSocket.Server.Protocol
{
    /// <summary>
    /// tcp协议接口
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IProtocol<TMessage> where TMessage : class, Messaging.IMessage
    {
        /// <summary>
        /// parse protocol message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        TMessage Parse(IConnection connection, ArraySegment<byte> buffer,
            int maxMessageSize, out int readlength);
    }
}