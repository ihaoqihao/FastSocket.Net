using System;

namespace Sodao.FastSocket.Server.Protocol
{
    /// <summary>
    /// thrift protocol
    /// </summary>
    public sealed class ThriftProtocol : IProtocol<Messaging.ThriftMessage>
    {
        /// <summary>
        /// parse
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">bad thrift protocol</exception>
        public Messaging.ThriftMessage Parse(SocketBase.IConnection connection, ArraySegment<byte> buffer,
            int maxMessageSize, out int readlength)
        {
            if (buffer.Count < 4)
            {
                readlength = 0;
                return null;
            }

            //获取message length
            var messageLength = SocketBase.Utils.NetworkBitConverter.ToInt32(buffer.Array, buffer.Offset);
            if (messageLength < 14) throw new BadProtocolException("bad thrift protocol");
            if (messageLength > maxMessageSize) throw new BadProtocolException("message is too long");

            readlength = messageLength + 4;
            if (buffer.Count < readlength)
            {
                readlength = 0;
                return null;
            }

            var payload = new byte[messageLength];
            Buffer.BlockCopy(buffer.Array, buffer.Offset + 4, payload, 0, messageLength);
            return new Messaging.ThriftMessage(payload);
        }
    }
}