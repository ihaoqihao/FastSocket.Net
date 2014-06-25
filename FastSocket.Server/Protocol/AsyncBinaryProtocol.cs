using Sodao.FastSocket.SocketBase;
using System;
using System.Text;

namespace Sodao.FastSocket.Server.Protocol
{
    /// <summary>
    /// 异步二进制协议
    /// 协议格式
    /// [Message Length(int32)][SeqID(int32)][Request|Response Flag Length(int16)][Request|Response Flag + Body Buffer]
    /// </summary>
    public sealed class AsyncBinaryProtocol : IProtocol<Command.AsyncBinaryCommandInfo>
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
        /// <exception cref="BadProtocolException">bad async binary protocl</exception>
        public Command.AsyncBinaryCommandInfo FindCommandInfo(IConnection connection, ArraySegment<byte> buffer,
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
            if (messageLength < 7) throw new BadProtocolException("bad async binary protocl");
            if (messageLength > maxMessageSize) throw new BadProtocolException("message is too long");

            readlength = messageLength + 4;
            if (buffer.Count < readlength)
            {
                readlength = 0;
                return null;
            }

            var seqID = SocketBase.Utils.NetworkBitConverter.ToInt32(payload, buffer.Offset + 4);
            var cmdNameLength = SocketBase.Utils.NetworkBitConverter.ToInt16(payload, buffer.Offset + 8);
            var strName = Encoding.UTF8.GetString(payload, buffer.Offset + 10, cmdNameLength);

            var dataLength = messageLength - 6 - cmdNameLength;
            byte[] data = null;
            if (dataLength > 0)
            {
                data = new byte[dataLength];
                Buffer.BlockCopy(payload, buffer.Offset + 10 + cmdNameLength, data, 0, dataLength);
            }
            return new Command.AsyncBinaryCommandInfo(strName, seqID, data);
        }
        #endregion
    }
}