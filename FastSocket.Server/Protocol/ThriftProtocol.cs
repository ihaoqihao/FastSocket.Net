using Sodao.FastSocket.SocketBase;
using System;

namespace Sodao.FastSocket.Server.Protocol
{
    /// <summary>
    /// thrift protocol
    /// </summary>
    public sealed class ThriftProtocol : IProtocol<Command.ThriftCommandInfo>
    {
        #region IProtocol Members
        /// <summary>
        /// find command
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="maxMessageSize"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">bad thrift protocol</exception>
        public Command.ThriftCommandInfo FindCommandInfo(IConnection connection, ArraySegment<byte> buffer,
            int maxMessageSize, out int readlength)
        {
            if (buffer.Count < 4)
            {
                readlength = 0;
                return null;
            }

            var payload = buffer.Array;
            //获取message length
            var messageLength = SocketBase.Utils.NetworkBitConverter.ToInt32(payload, buffer.Offset);
            if (messageLength < 14) throw new BadProtocolException("bad thrift protocol");
            if (messageLength > maxMessageSize) throw new BadProtocolException("message is too long");

            readlength = messageLength + 4;
            if (buffer.Count < readlength)
            {
                readlength = 0;
                return null;
            }

            var data = new byte[messageLength];
            Buffer.BlockCopy(payload, buffer.Offset + 4, data, 0, messageLength);

            return new Command.ThriftCommandInfo(data);
        }
        #endregion
    }
}