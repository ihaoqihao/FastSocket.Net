using System;
using System.Text;

namespace Sodao.FastSocket.Client.Protocol
{
    /// <summary>
    /// 异步二进制协议
    /// 协议格式
    /// [Message Length(int32)][SeqID(int32)][Request|Response Flag Length(int16)][Request|Response Flag + Body Buffer]
    /// </summary>
    public sealed class AsyncBinaryProtocol : IProtocol<Response.AsyncBinaryResponse>
    {
        #region IProtocol Members
        /// <summary>
        /// find response
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="buffer"></param>
        /// <param name="readlength"></param>
        /// <returns></returns>
        /// <exception cref="BadProtocolException">bad async binary protocl</exception>
        public Response.AsyncBinaryResponse FindResponse(SocketBase.IConnection connection, ArraySegment<byte> buffer, out int readlength)
        {
            if (buffer.Count < 4)
            {
                readlength = 0;
                return null;
            }

            //获取message length
            var messageLength = SocketBase.Utils.NetworkBitConverter.ToInt32(buffer.Array, buffer.Offset);
            if (messageLength < 7) throw new BadProtocolException("bad async binary protocl");

            readlength = messageLength + 4;
            if (buffer.Count < readlength)
            {
                readlength = 0;
                return null;
            }

            var seqID = SocketBase.Utils.NetworkBitConverter.ToInt32(buffer.Array, buffer.Offset + 4);
            var flagLength = SocketBase.Utils.NetworkBitConverter.ToInt16(buffer.Array, buffer.Offset + 8);
            var strName = Encoding.UTF8.GetString(buffer.Array, buffer.Offset + 10, flagLength);

            var dataLength = messageLength - 6 - flagLength;
            byte[] data = null;
            if (dataLength > 0)
            {
                data = new byte[dataLength];
                Buffer.BlockCopy(buffer.Array, buffer.Offset + 10 + flagLength, data, 0, dataLength);
            }
            return new Response.AsyncBinaryResponse(strName, seqID, data);
        }
        #endregion
    }
}